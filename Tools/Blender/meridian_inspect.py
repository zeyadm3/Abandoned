"""Read exported FBX bounds/hierarchies for the art handoff; never runs Unity."""
import bpy, json, math, os, sys
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
repo=args[0] if args else os.path.abspath(os.path.join(os.path.dirname(__file__),'../..'))
folder=os.path.join(repo,'Game/Assets/_Project/Art/Custom/Environment/Meridian')
report={}; errors=[]
for filename in sorted(os.listdir(folder)):
    if not filename.endswith('.fbx'):continue
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    for mat in list(bpy.data.materials):bpy.data.materials.remove(mat)
    bpy.ops.import_scene.fbx(filepath=os.path.join(folder,filename),use_anim=False)
    vertices=[]; slots=set(); renderers=[]
    for obj in bpy.context.scene.objects:
        if obj.type!='MESH':continue
        renderers.append(obj.name);slots.update(mat.name for mat in obj.data.materials if mat)
        for vertex in obj.data.vertices:
            p=obj.matrix_world@vertex.co
            vertices.append((p.x,p.z,p.y))
            if not all(math.isfinite(v) for v in p):errors.append(filename+': non-finite vertex')
    if not vertices:errors.append(filename+': empty model');continue
    bounds=[[min(v[i] for v in vertices) for i in range(3)],
            [max(v[i] for v in vertices) for i in range(3)]]
    size=[round(bounds[1][i]-bounds[0][i],4) for i in range(3)]
    if max(size)>40:errors.append(filename+': unexpectedly oversized '+str(size))
    if filename=='FloorTerrazzo.fbx' and (abs(size[0]-4)>.03 or abs(size[2]-4)>.03):
        errors.append(filename+': floor must retain its four metre horizontal dimensions')
    report[filename[:-4]]={'bounds':bounds,'size':size,'materials':sorted(slots),'renderers':sorted(renderers)}
with open(os.path.join(folder,'MeridianGeometry.json'),'w') as f:json.dump(report,f,indent=2)
print('MERIDIAN_INSPECTION',len(report),'models',len(errors),'problems')
for key in ('FloorTerrazzo','FloorTerrazzoFractured','StorefrontEntry','Column','Escalator','ServiceStair','HaulRamp','GlassRail4m','FluorescentFixture'):
    print(key,report.get(key))
if errors:
    for error in sorted(set(errors)):print('MERIDIAN_ERROR',error)
    raise RuntimeError('Exported asset inspection found invalid geometry')
