"""Run: /Applications/Blender.app/Contents/MacOS/Blender -b --factory-startup --python art/blender_trial/build_tile.py
Only Blender's bundled Python modules are required. No external textures.
"""
import bpy, bmesh, json, math, struct
from pathlib import Path
from mathutils import Vector
from mathutils.geometry import delaunay_2d_cdt

OUT = Path(__file__).resolve().parent
REFERENCE = OUT.parent / 'style_trial/board_4_lowpoly.png'
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for data in list(bpy.data.materials):
    bpy.data.materials.remove(data)
# Exact sRGB pixels sampled from the approved reference, coordinates from upper left.
SAMPLES = [('Ice pale',(402,350)),('Ice light',(390,370)),('Ice blue',(420,420)),
           ('Ice facet',(460,410)),('Ice bevel',(375,446)),('Ice side',(477,442)),
           ('Ice edge',(488,411)),('Snow white',(480,22)),('Snow shade',(469,24)),
           ('Snow blue',(111,30))]
ref = bpy.data.images.load(str(REFERENCE), check_existing=False)
pixels = list(ref.pixels[:])
# Blender image pixel buffer for this PNG preserves the encoded sRGB values.
palette=[]
for name,(x,y) in SAMPLES:
    i=((ref.size[1]-1-y)*ref.size[0]+x)*4
    rgb=tuple(round(pixels[i+k]*255) for k in range(3))
    palette.append({'name':name,'pixel':[x,y],'hex':'#'+''.join(f'{c:02X}' for c in rgb)})
bpy.data.images.remove(ref)
def linear(v):
    return v/12.92 if v<=0.04045 else ((v+0.055)/1.055)**2.4
materials=[]
for entry in palette:
    h=entry['hex'][1:]; color=tuple(linear(int(h[k:k+2],16)/255) for k in (0,2,4))
    mat=bpy.data.materials.new(entry['name']); mat.diffuse_color=(*color,1)
    mat.use_nodes=True
    bs=mat.node_tree.nodes.get('Principled BSDF')
    bs.inputs['Base Color'].default_value=(*color,1)
    bs.inputs['Roughness'].default_value=.78
    materials.append(mat)
verts=[]; faces=[]; mids=[]
def face(indices, material):
    faces.append(indices); mids.append(material)
def ring(half,clip,z):
    points=[(-half+clip,-half),(half-clip,-half),(half,-half+clip),(half,half-clip),
            (half-clip,half),(-half+clip,half),(-half,half-clip),(-half,-half+clip)]
    ids=[]
    for x,y in points:
        ids.append(len(verts)); verts.append((x,y,z))
    return ids
# Bevels are explicit geometry; triangulation is still applied and evaluated below.
rings=[ring(.465,.055,0),ring(.5,.075,.035),ring(.5,.075,.215),ring(.465,.055,.25)]
face(list(reversed(rings[0])),5)
for level in range(3):
    a,b=rings[level:level+2]
    for i in range(8):
        j=(i+1)%8
        face([a[i],a[j],b[j]], [4,5,0][level] if i%3 else [6,6,1][level])
        face([a[i],b[j],b[i]], [4,6,1][level] if i%2 else [6,5,0][level])
# Coplanar irregular triangles create the broad crystalline top without bumps.
top=rings[-1]
for p in [(-.24,-.19),(.10,-.25),(.28,.07),(-.04,.12),(-.27,.29)]:
    top=top+[len(verts)]; verts.append((*p,.25))
coords=[Vector(verts[i][:2]) for i in top]
result=delaunay_2d_cdt(coords,[],[],0,1e-7)
for n,tri in enumerate(result[2]):
    face([top[i] for i in tri],[0,1,2,0,3,1,2,0,1,3,0][n%11])
# Small faceted closed snow chunk, slightly inset into the ice for a clean seam.
outline=[(-.37,.15),(-.23,.10),(-.10,.18),(-.08,.31),(-.16,.42),(-.33,.43),(-.43,.32)]
bottom=[]; upper=[]
for x,y in outline:
    bottom.append(len(verts)); verts.append((x,y,.244))
for i,(x,y) in enumerate(outline):
    upper.append(len(verts)); verts.append((x*.93-.014,y*.94+.017,[.272,.269,.275,.271,.28,.273,.27][i]))
center=len(verts); verts.append((-.25,.29,.30))
face(list(reversed(bottom)),8)
for i in range(7):
    j=(i+1)%7
    face([bottom[i],bottom[j],upper[j],upper[i]],8 if i%3 else 9)
    face([upper[i],upper[j],center],7 if i%3 else 8)
mesh=bpy.data.meshes.new('TileGeometry'); mesh.from_pydata(verts,[],faces); mesh.update()
obj=bpy.data.objects.new('IceBlockTile',mesh); bpy.context.collection.objects.link(obj)
for mat in materials: mesh.materials.append(mat)
for poly,mi in zip(mesh.polygons,mids): poly.material_index=mi; poly.use_smooth=False
bpy.context.view_layer.objects.active=obj; obj.select_set(True)
tri=obj.modifiers.new('Applied triangulation','TRIANGULATE')
bpy.ops.object.modifier_apply(modifier=tri.name)
# Recalculate outward normals for both closed connected components.
bm=bmesh.new(); bm.from_mesh(obj.data); bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.to_mesh(obj.data); bm.free()

def check_evaluated():
    deps=bpy.context.evaluated_depsgraph_get(); evaluated=obj.evaluated_get(deps)
    me=evaluated.to_mesh(); me.calc_loop_triangles()
    bm=bmesh.new(); bm.from_mesh(me)
    coords=[obj.matrix_world@v.co for v in me.vertices]
    lo=[min(v[k] for v in coords) for k in range(3)]
    hi=[max(v[k] for v in coords) for k in range(3)]
    ice_indices={i for p in me.polygons if p.material_index<7 for i in p.vertices}
    snow_indices={i for p in me.polygons if p.material_index>=7 for i in p.vertices}
    icecoords=[coords[i] for i in ice_indices]
    snowcoords=[coords[i] for i in snow_indices]
    ice_size=[round(max(v[k] for v in icecoords)-min(v[k] for v in icecoords),6) for k in range(3)]
    top_z=[v.z for v in icecoords if v.z>.249999]
    components=0; pending=set(bm.verts)
    while pending:
        components+=1; stack=[pending.pop()]
        while stack:
            for edge in stack.pop().link_edges:
                for v in edge.verts:
                    if v in pending: pending.remove(v); stack.append(v)
    result={'triangle_count':len(me.loop_triangles),'boundary_edges':sum(e.is_boundary for e in bm.edges),
        'non_manifold_edges':sum(not e.is_manifold for e in bm.edges),'connected_closed_shells':components,
        'materials_used':len(set(p.material_index for p in me.polygons)),
        'smooth_shaded_faces':sum(p.use_smooth for p in me.polygons),
        'bounds_min':[round(x,6) for x in lo],'bounds_max':[round(x,6) for x in hi],
        'size':[round(hi[k]-lo[k],6) for k in range(3)],'ice_block_size':ice_size,'snow_height_max':round(max(v.z for v in snowcoords),6),
        'ice_top_z_spread':round(max(top_z)-min(top_z),8),
        'ice_materials_used':sorted({p.material_index for p in me.polygons if p.material_index<7}),
        'snow_materials_used':sorted({p.material_index for p in me.polygons if p.material_index>=7}),
        'origin':list(obj.location),'top_surface_z':.25,'remaining_modifiers':len(obj.modifiers),'palette':palette}
    result['passes']={'triangles':result['triangle_count']<=2000,'watertight':result['boundary_edges']==result['non_manifold_edges']==0,
        'materials_and_flat_shading':result['materials_used']>=2 and result['smooth_shaded_faces']==0,
        'flat_ice_top':result['ice_top_z_spread']==0,
        'dimensions_and_origin':result['size']==[1.,1.,.3] and result['ice_block_size']==[1.,1.,.25] and list(obj.location)==[0.,0.,0.]}
    bm.free(); evaluated.to_mesh_clear(); return result
checks=check_evaluated()
bpy.ops.export_scene.gltf(filepath=str(OUT/'tile.glb'),export_format='GLB',use_selection=True,export_yup=True,export_apply=True)
# Verify the serialized GLB primitive POSITION bounds after the actual axis conversion.
blob=(OUT/'tile.glb').read_bytes(); jsonlen=struct.unpack_from('<I',blob,12)[0]
gltf=json.loads(blob[20:20+jsonlen]); access=[gltf['accessors'][p['attributes']['POSITION']] for m in gltf['meshes'] for p in m['primitives']]
checks['glb']={'up_axis':'Y','position_min':[round(min(a['min'][k] for a in access),6) for k in range(3)],
               'position_max':[round(max(a['max'][k] for a in access),6) for k in range(3)],'mesh_count':len(gltf['meshes'])}
checks['passes']['y_up_export']=checks['glb']['position_min']==[-.5,0.,-.5] and checks['glb']['position_max']==[.5,.3,.5]
scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=48
scene.cycles.seed=0
# CPU rendering avoids a headless Metal initialization stall on this host.
scene.cycles.device='CPU'
scene.render.resolution_x=512; scene.render.resolution_y=512; scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='Standard'; scene.view_settings.look='None'; scene.view_settings.exposure=0.0
scene.world.color=(.65,.65,.65)
scene.world.use_nodes=True; scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.73,.77,.82,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.8
light_data=bpy.data.lights.new('Neutral softbox','AREA'); light_data.energy=180; light_data.shape='DISK'; light_data.size=4
light=bpy.data.objects.new('Neutral softbox',light_data); scene.collection.objects.link(light); light.location=(-2,-3,5)
light.rotation_euler=(Vector((0,0,.1))-light.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('Camera'); cam=bpy.data.objects.new('Camera',camdata); scene.collection.objects.link(cam); scene.camera=cam
camdata.type='ORTHO'; camdata.ortho_scale=1.65
for name,position,target in [('front',(0,-3,.125),(0,0,.125)),('top',(0,0,3),(0,0,.12)),('iso',(2,-3,2.8),(0,0,.13))]:
    cam.location=position; cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(OUT/f'render_{name}.png'); bpy.ops.render.render(write_still=True)
    im=bpy.data.images.load(scene.render.filepath,check_existing=False)
    checks.setdefault('renders',{})[name]={'width':im.size[0],'height':im.size[1]}; bpy.data.images.remove(im)
checks['passes']['renders']=all(v=={'width':512,'height':512} for v in checks['renders'].values())
(OUT/'check.json').write_text(json.dumps(checks,indent=2,sort_keys=True)+'\n')
print('CHECK_RESULT',json.dumps(checks,sort_keys=True))
assert all(checks['passes'].values()), 'Tile requirement failed; inspect check.json'
