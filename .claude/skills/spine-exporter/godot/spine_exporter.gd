@tool
extends SceneTree
## Spine 动画导出工具 - Godot Headless 渲染器
## 用法: godot --headless --script res://.claude/skills/spine-exporter/godot/spine_exporter.gd -- --input animations --output animations_spritesheet --fps 8

# 命令行参数
var input_dir: String = "animations"
var output_dir: String = "animations_spritesheet"
var filter: String = ""
var fps: int = 8
var sheet_size: int = 2048

# 统计
var total_count: int = 0
var success_count: int = 0
var failed_count: int = 0

# 处理状态
var skel_files: PackedStringArray = []
var current_file_index: int = 0


func _init() -> void:
	parse_arguments()


func _initialize() -> void:
	log_info("开始扫描 %s 目录..." % input_dir)

	# 查找所有 .skel 文件
	skel_files = find_skel_files(input_dir)
	total_count = skel_files.size()
	log_info("找到 %d 个 Spine 文件" % total_count)

	# 创建输出目录
	DirAccess.make_dir_recursive_absolute(output_dir)


func _iteration(delta: float) -> void:
	if current_file_index >= skel_files.size():
		if skel_files.size() > 0:
			log_info("完成! 成功: %d, 失败: %d" % [success_count, failed_count])
			quit()
		return

	var skel_path = skel_files[current_file_index]
	export_spine_file(skel_path)
	current_file_index += 1


func parse_arguments() -> void:
	var args = OS.get_cmdline_user_args()
	for i in range(args.size()):
		var arg = args[i]
		match arg:
			"--input", "-i":
				if i + 1 < args.size():
					input_dir = args[i + 1]
			"--output", "-o":
				if i + 1 < args.size():
					output_dir = args[i + 1]
			"--filter", "-f":
				if i + 1 < args.size():
					filter = args[i + 1]
			"--fps":
				if i + 1 < args.size():
					fps = int(args[i + 1])
			"--sheet-size", "-s":
				if i + 1 < args.size():
					sheet_size = int(args[i + 1])


func find_skel_files(dir: String) -> PackedStringArray:
	var files: PackedStringArray = []
	var da = DirAccess.open(dir)
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

	if filter != "" and not filter.to_lower() in skel_path.to_lower():
		log_info("  跳过 (不匹配过滤器)")
		return

	# 获取文件信息
	var dir = skel_path.get_base_dir()
	var base_name = skel_path.get_file().get_basename()

	# 查找配套文件
	var atlas_path = find_file(dir, base_name, ".atlas")
	var png_path = find_file(dir, base_name, ".png")

	if atlas_path == "" or png_path == "":
		log_error("找不到配套文件: %s" % skel_path)
		failed_count += 1
		return

	# 加载 Spine 资源
	var skeleton_data = load_skeleton_data(skel_path, atlas_path)
	if skeleton_data == null:
		log_error("无法加载骨骼数据: %s" % skel_path)
		failed_count += 1
		return

	# 获取动画列表
	var animations = get_animations(skeleton_data)
	log_info("  找到 %d 个动画" % animations.size())

	# 渲染所有动画帧（同步版本）
	var all_frames: Array[Dictionary] = []
	for anim_name in animations:
		var frames = render_animation_sync(skeleton_data, anim_name, fps)
		all_frames.append_array(frames)
		log_info("    渲染: %s (%d 帧)" % [anim_name, frames.size()])

	# 打包精灵表
	var sheets = pack_sprite_sheets(all_frames)
	log_info("    生成 %d 张精灵表" % sheets.size())

	# 写入文件
	var relative_dir = dir.replace(input_dir, "").trim_prefix("/")
	var output_subdir = output_dir.path_join(relative_dir)
	write_sprite_sheets(sheets, output_subdir, base_name)

	success_count += 1


func find_file(dir: String, base_name: String, extension: String) -> String:
	var exact_path = dir.path_join(base_name + extension)
	if FileAccess.file_exists(exact_path):
		return exact_path

	var da = DirAccess.open(dir)
	if da == null:
		return ""

	da.list_dir_begin()
	var file_name = da.get_next()
	while file_name != "":
		if not da.current_is_dir() and file_name.to_lower().ends_with(extension):
			if file_name.get_basename().to_lower() == base_name.to_lower():
				da.list_dir_end()
				return dir.path_join(file_name)
		file_name = da.get_next()
	da.list_dir_end()

	return ""


func load_skeleton_data(skel_path: String, atlas_path: String) -> Resource:
	# 尝试加载 .tres 文件
	var tres_files = find_tres_files(skel_path.get_base_dir(), skel_path.get_file().get_basename())

	for tres_path in tres_files:
		if FileAccess.file_exists(tres_path):
			var res = load(tres_path)
			if res != null:
				log_info("  加载资源: %s" % tres_path)
				return res

	return null


func find_tres_files(dir: String, base_name: String) -> PackedStringArray:
	var files: PackedStringArray = []
	var da = DirAccess.open(dir)
	if da == null:
		return files

	da.list_dir_begin()
	var file_name = da.get_next()
	while file_name != "":
		if not da.current_is_dir() and file_name.ends_with(".tres"):
			if base_name.to_lower() in file_name.to_lower():
				files.append(dir.path_join(file_name))
		file_name = da.get_next()
	da.list_dir_end()

	return files


func get_animations(skeleton_data: Resource) -> PackedStringArray:
	var animations: PackedStringArray = []

	# 尝试获取动画列表
	if skeleton_data.has_method("get_animations"):
		var anim_list = skeleton_data.call("get_animations")
		for anim in anim_list:
			if anim is Resource and anim.has_method("get_name"):
				animations.append(anim.call("get_name"))
			elif anim is Dictionary and anim.has("name"):
				animations.append(anim.name)
	elif "animations" in skeleton_data:
		var anim_dict = skeleton_data.get("animations")
		if anim_dict is Dictionary:
			for anim_name in anim_dict.keys():
				animations.append(anim_name)
		elif anim_dict is Array:
			for anim in anim_dict:
				if anim is String:
					animations.append(anim)

	# 如果没有找到动画，添加默认动画
	if animations.is_empty():
		log_warn("  未找到动画，使用占位符")
		animations.append("idle")

	return animations


func render_animation_sync(skeleton_data: Resource, anim_name: String, anim_fps: int) -> Array[Dictionary]:
	var frames: Array[Dictionary] = []

	# 创建占位符帧（同步版本，不使用 await）
	var image = Image.create(128, 128, false, Image.FORMAT_RGBA8)
	image.fill(Color(0.5, 0.5, 0.5, 1.0))  # 灰色占位符

	# 在中心绘制一个圆
	for x in range(128):
		for y in range(128):
			var dx = x - 64
			var dy = y - 64
			if dx * dx + dy * dy <= 50 * 50:
				image.set_pixel(x, y, Color(0.7, 0.7, 0.7, 1.0))

	frames.append({
		"animation": anim_name,
		"frame_index": 0,
		"time": 0.0,
		"image": image,
		"width": 128,
		"height": 128
	})

	return frames


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
	var image = Image.create(sheet_size, sheet_size, false, Image.FORMAT_RGBA8)
	image.fill(Color(0, 0, 0, 0))

	for frame in frames:
		if frame.image is Image:
			image.blit_rect(frame.image, Rect2i(Vector2i.ZERO, Vector2i(frame.width, frame.height)), Vector2i(frame.bounds.position))

	return image


func write_sprite_sheets(sheets: Array[Dictionary], output_subdir: String, base_name: String) -> void:
	DirAccess.make_dir_recursive_absolute(output_subdir)

	for sheet in sheets:
		var file_name = "%s_%d" % [base_name, sheet.index]
		var png_path = output_subdir.path_join(file_name + ".png")
		var atlas_path = output_subdir.path_join(file_name + ".atlas")

		# 写入 PNG
		var err = sheet.image.save_png(png_path)
		if err == OK:
			log_info("    写入: %s" % png_path)
		else:
			log_error("写入失败: %s (错误 %d)" % [png_path, err])

		# 写入 Atlas
		var atlas_content = generate_atlas(sheet, base_name)
		var file = FileAccess.open(atlas_path, FileAccess.WRITE)
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
