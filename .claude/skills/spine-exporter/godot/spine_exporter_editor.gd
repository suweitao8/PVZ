@tool
extends EditorScript
## Spine 动画导出工具 - 在 Godot 编辑器中运行
## 使用方法: 在 Godot 编辑器中打开此脚本，点击"运行"按钮

# 命令行参数
var input_dir: String = "animations"
var output_dir: String = "animations_spritesheet"
var filter: String = ""  # 留空处理所有，或填入如 "twig_slime_s" 只处理特定动画
var fps: int = 8
var sheet_size: int = 2048

# 统计
var total_count: int = 0
var success_count: int = 0
var failed_count: int = 0


func _run() -> void:
	print("=== Spine 动画导出工具 ===")
	print("输入目录: %s" % input_dir)
	print("输出目录: %s" % output_dir)
	print("过滤器: %s" % (filter if filter else "无"))
	print("FPS: %d" % fps)
	print("")

	export_all()


func export_all() -> void:
	log_info("开始扫描 %s 目录..." % input_dir)

	# 查找所有 .skel 文件
	var skel_files = find_skel_files(input_dir)
	total_count = skel_files.size()
	log_info("找到 %d 个 Spine 文件" % total_count)

	# 创建输出目录
	DirAccess.make_dir_recursive_absolute("res://%s" % output_dir)

	# 处理每个文件
	for skel_path in skel_files:
		if filter != "" and not filter.to_lower() in skel_path.to_lower():
			continue
		export_spine_file(skel_path)

	# 输出报告
	log_info("完成! 成功: %d, 失败: %d" % [success_count, failed_count])


func find_skel_files(dir: String) -> PackedStringArray:
	var files: PackedStringArray = []
	var da = DirAccess.open("res://%s" % dir)
	if da == null:
		return files

	da.list_dir_begin()
	var file_name = da.get_next()
	while file_name != "":
		if da.current_is_dir():
			var sub_files = find_skel_files(dir.path_join(file_name))
			for f in sub_files:
				files.append(dir.path_join(file_name).path_join(f))
		elif file_name.ends_with(".skel"):
			files.append(dir.path_join(file_name))
		file_name = da.get_next()
	da.list_dir_end()

	return files


func export_spine_file(skel_path: String) -> void:
	log_info("处理: %s" % skel_path)

	# 获取文件信息
	var dir = skel_path.get_base_dir()
	var base_name = skel_path.get_file().get_basename()

	# 查找 .tres 资源文件
	var tres_path = find_tres_file(dir, base_name)
	if tres_path == "":
		log_error("找不到 .tres 资源文件: %s" % skel_path)
		failed_count += 1
		return

	# 加载 Spine 资源
	var skeleton_data = load(tres_path)
	if skeleton_data == null:
		log_error("无法加载骨骼数据: %s" % tres_path)
		failed_count += 1
		return

	# 创建 SpineSprite 进行渲染
	var spine_sprite = create_spine_sprite(skeleton_data)
	if spine_sprite == null:
		log_error("无法创建 SpineSprite")
		failed_count += 1
		return

	# 获取动画列表
	var animations = get_animations(spine_sprite)
	log_info("  找到 %d 个动画" % animations.size())

	if animations.is_empty():
		log_warn("  没有找到动画")
		spine_sprite.queue_free()
		failed_count += 1
		return

	# 渲染所有动画帧
	var all_frames: Array[Dictionary] = []
	for anim_name in animations:
		var frames = render_animation(spine_sprite, anim_name)
		all_frames.append_array(frames)
		log_info("    渲染: %s (%d 帧)" % [anim_name, frames.size()])

	# 打包精灵表
	var sheets = pack_sprite_sheets(all_frames)
	log_info("    生成 %d 张精灵表" % sheets.size())

	# 写入文件
	var relative_dir = dir.replace(input_dir, "").trim_prefix("/")
	var output_subdir = output_dir.path_join(relative_dir)
	write_sprite_sheets(sheets, output_subdir, base_name)

	spine_sprite.queue_free()
	success_count += 1


func find_tres_file(dir: String, base_name: String) -> String:
	var da = DirAccess.open("res://%s" % dir)
	if da == null:
		return ""

	da.list_dir_begin()
	var file_name = da.get_next()
	while file_name != "":
		if not da.current_is_dir() and file_name.ends_with(".tres"):
			if base_name.to_lower() in file_name.to_lower():
				da.list_dir_end()
				return dir.path_join(file_name)
		file_name = da.get_next()
	da.list_dir_end()

	return ""


func create_spine_sprite(skeleton_data: Resource) -> Node:
	# 检查 SpineSprite 类是否可用
	if not ClassDB.class_exists("SpineSprite"):
		log_error("SpineSprite 类不可用，请确保 Spine GDExtension 已正确安装")
		return null

	var spine_sprite = ClassDB.instantiate("SpineSprite")
	spine_sprite.set("skeleton_data_res", skeleton_data)

	# 添加到场景树以便渲染
	var temp_node = Node.new()
	temp_node.add_child(spine_sprite)
	Engine.get_main_loop().root.add_child(temp_node)

	return spine_sprite


func get_animations(spine_sprite: Node) -> PackedStringArray:
	var animations: PackedStringArray = []

	# 尝试获取动画列表
	var skeleton = spine_sprite.call("get_skeleton")
	if skeleton == null:
		return animations

	var skeleton_data = skeleton.call("get_data")
	if skeleton_data == null:
		return animations

	var anim_list = skeleton_data.call("get_animations")
	for anim in anim_list:
		var name = anim.call("get_name") if anim.has_method("get_name") else str(anim)
		if name != "" and not animations.has(name):
			animations.append(name)

	return animations


func render_animation(spine_sprite: Node, anim_name: String) -> Array[Dictionary]:
	var frames: Array[Dictionary] = []

	# 设置动画
	var anim_state = spine_sprite.call("get_animation_state")
	if anim_state == null:
		return frames

	# 获取动画时长
	var track = anim_state.call("set_animation", 0, anim_name, false)
	if track == null:
		return frames

	await Engine.get_main_loop().process_frame

	var duration = track.call("get_animation_end")
	var frame_count = max(1, int(ceil(duration * fps)))
	var frame_interval = 1.0 / fps

	# 获取骨骼包围盒
	var skeleton = spine_sprite.call("get_skeleton")
	var bounds = get_skeleton_bounds(skeleton)
	var frame_width = max(64, int(bounds.size.x) + 20)
	var frame_height = max(64, int(bounds.size.y) + 20)

	# 创建 SubViewport 渲染
	var viewport = SubViewport.new()
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	viewport.render_target_clear_mode = SubViewport.CLEAR_MODE_ALWAYS
	viewport.transparent_bg = true
	viewport.size = Vector2i(frame_width, frame_height)

	var camera = Camera2D.new()
	camera.enabled = true
	camera.position = bounds.position + bounds.size / 2
	camera.zoom = Vector2(1, 1)

	viewport.add_child(camera)

	# 克隆 SpineSprite 到 viewport
	var spine_clone = create_spine_sprite_for_viewport(spine_sprite.get("skeleton_data_res"))
	viewport.add_child(spine_clone)

	Engine.get_main_loop().root.add_child(viewport)

	# 渲染每一帧
	for i in range(frame_count):
		var time = i * frame_interval

		# 设置动画时间
		track.call("set_track_time", time)
		anim_state.call("update", 0)
		anim_state.call("apply", skeleton)
		skeleton.call("update_world_transform")

		# 同步到克隆的骨骼
		sync_skeleton_state(skeleton, spine_clone.call("get_skeleton"))

		# 等待渲染
		await Engine.get_main_loop().process_frame
		await Engine.get_main_loop().process_frame

		# 获取图像
		var image = viewport.get_texture().get_image()
		if image != null:
			frames.append({
				"animation": anim_name,
				"frame_index": i,
				"time": time,
				"image": image,
				"width": image.get_width(),
				"height": image.get_height()
			})

	viewport.queue_free()

	return frames


func create_spine_sprite_for_viewport(skeleton_data: Resource) -> Node:
	if not ClassDB.class_exists("SpineSprite"):
		return null

	var spine_sprite = ClassDB.instantiate("SpineSprite")
	spine_sprite.set("skeleton_data_res", skeleton_data)

	return spine_sprite


func sync_skeleton_state(source_skeleton: Node, target_skeleton: Node) -> void:
	if source_skeleton == null or target_skeleton == null:
		return

	# 复制骨骼状态
	target_skeleton.call("set_position", source_skeleton.call("get_position"))
	target_skeleton.call("set_rotation", source_skeleton.call("get_rotation"))
	target_skeleton.call("set_scale", source_skeleton.call("get_scale"))


func get_skeleton_bounds(skeleton: Node) -> Rect2:
	if skeleton == null:
		return Rect2(Vector2(0, 0), Vector2(128, 128))

	# 尝试获取包围盒
	if skeleton.has_method("get_bounds"):
		return skeleton.call("get_bounds")

	# 默认包围盒
	return Rect2(Vector2(-64, -64), Vector2(128, 128))


func pack_sprite_sheets(frames: Array[Dictionary]) -> Array[Dictionary]:
	var sheets: Array[Dictionary] = []
	if frames.is_empty():
		return sheets

	var current_frames: Array[Dictionary] = []
	var current_x: int = 0
	var current_y: int = 0
	var row_height: int = 0
	var sheet_index: int = 0

	for frame in frames:
		var frame_width = frame.width
		var frame_height = frame.height

		# 换行检查
		if current_x + frame_width > sheet_size:
			current_x = 0
			current_y += row_height
			row_height = 0

		# 新建精灵表检查
		if current_y + frame_height > sheet_size:
			sheets.append({
				"index": sheet_index,
				"frames": current_frames.duplicate(),
				"image": _create_sheet_image(current_frames)
			})
			sheet_index += 1
			current_frames.clear()
			current_x = 0
			current_y = 0
			row_height = 0

		# 设置帧位置
		frame["bounds"] = Rect2i(current_x, current_y, frame_width, frame_height)
		current_frames.append(frame)

		current_x += frame_width
		row_height = max(row_height, frame_height)

	# 添加最后的精灵表
	if not current_frames.is_empty():
		sheets.append({
			"index": sheet_index,
			"frames": current_frames.duplicate(),
			"image": _create_sheet_image(current_frames)
		})

	return sheets


func _create_sheet_image(frames: Array[Dictionary]) -> Image:
	if frames.is_empty():
		return Image.create(sheet_size, sheet_size, false, Image.FORMAT_RGBA8)

	var image = Image.create(sheet_size, sheet_size, false, Image.FORMAT_RGBA8)
	image.fill(Color(0, 0, 0, 0))

	for frame in frames:
		if frame.image is Image:
			image.blit_rect(frame.image, Rect2i(Vector2i.ZERO, Vector2i(frame.width, frame.height)), Vector2i(frame.bounds.position))

	return image


func write_sprite_sheets(sheets: Array[Dictionary], output_subdir: String, base_name: String) -> void:
	DirAccess.make_dir_recursive_absolute("res://%s" % output_subdir)

	for sheet in sheets:
		var file_name = "%s_%d" % [base_name, sheet.index]
		var png_path = output_subdir.path_join(file_name + ".png")
		var atlas_path = output_subdir.path_join(file_name + ".atlas")

		# 写入 PNG
		var err = sheet.image.save_png("res://%s" % png_path)
		if err == OK:
			log_info("    写入: %s" % png_path)
		else:
			log_error("写入失败: %s (错误 %d)" % [png_path, err])

		# 写入 Atlas
		var atlas_content = generate_atlas(sheet, base_name)
		var file = FileAccess.open("res://%s" % atlas_path, FileAccess.WRITE)
		if file:
			file.store_string(atlas_content)
			file.close()
			log_info("    写入: %s" % atlas_path)


func generate_atlas(sheet: Dictionary, base_name: String) -> String:
	var content = "# %s_%d.atlas\n\n" % [base_name, sheet.index]

	# Metadata
	content += "[metadata]\n"
	content += "source: %s.skel\n" % base_name
	content += "fps: %d\n" % fps
	content += "created: %s\n\n" % Time.get_datetime_string_from_system()

	# Texture
	content += "[texture]\n"
	content += "file: %s_%d.png\n" % [base_name, sheet.index]
	content += "size: %d, %d\n\n" % [sheet_size, sheet_size]

	# Animations
	var anim_groups: Dictionary = {}
	for frame in sheet.frames:
		var anim_name = frame.animation
		if not anim_groups.has(anim_name):
			anim_groups[anim_name] = []
		anim_groups[anim_name].append(frame)

	for anim_name in anim_groups:
		var anim_frames = anim_groups[anim_name]
		anim_frames.sort_custom(func(a, b): return a.frame_index < b.frame_index)

		content += "[animation: %s]\n" % anim_name
		content += "loop: %s\n" % ("true" if is_loop_animation(anim_name) else "false")
		content += "frames: %d\n" % anim_frames.size()

		for frame in anim_frames:
			content += "frame_%02d:\n" % frame.frame_index
			content += "  bounds: %d, %d, %d, %d\n" % [
				frame.bounds.position.x,
				frame.bounds.position.y,
				frame.bounds.size.x,
				frame.bounds.size.y
			]
		content += "\n"

	return content


func is_loop_animation(anim_name: String) -> bool:
	var loop_patterns = ["idle", "walk", "run", "fly", "swim", "float", "hover", "loop"]
	for pattern in loop_patterns:
		if pattern.to_lower() in anim_name.to_lower():
			return true
	return false


func log_info(msg: String) -> void:
	print("[INFO] %s" % msg)


func log_warn(msg: String) -> void:
	push_warning("[WARN] %s" % msg)


func log_error(msg: String) -> void:
	push_error("[ERROR] %s" % msg)
