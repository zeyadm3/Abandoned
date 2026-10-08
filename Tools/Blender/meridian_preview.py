"""Optional original-kit still render for art review; no Unity or gameplay execution."""
import bpy, os, sys, math
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
repo=args[0] if args else os.path.abspath(os.path.join(os.path.dirname(__file__),'../..'))
output=args[1] if len(args)>1 else '/tmp/meridian_kit_preview.png'
folder=os.path.join(repo,'Game/Assets/_Project/Art/Custom/Environment/Meridian')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
cache={}

def material(key):
    if key in cache:return cache[key]
    mat=bpy.data.materials.new('Preview_'+key);mat.use_nodes=True
    nodes=mat.node_tree.nodes;links=mat.node_tree.links;shader=nodes.get('Principled BSDF')
    for suffix in ('BaseColor','Normal','Mask'):
        tex=nodes.new('ShaderNodeTexImage')
        tex.image=bpy.data.images.load(os.path.join(folder,'Textures',key+'_'+suffix+'.png'),check_existing=True)
        tex.image.colorspace_settings.name='sRGB' if suffix=='BaseColor' else 'Non-Color'
        if suffix=='BaseColor':
            links.new(tex.outputs['Color'],shader.inputs['Base Color'])
            if key in ('MALL_Glass','MALL_Water','MALL_Moss','MALL_Leaves','MALL_Graffiti','MALL_Scorch'):
                links.new(tex.outputs['Alpha'],shader.inputs['Alpha']);mat.surface_render_method='DITHERED'
            if key=='MALL_FixtureGlow':
                links.new(tex.outputs['Color'],shader.inputs['Emission Color']);shader.inputs['Emission Strength'].default_value=.7
        elif suffix=='Normal':
            normal=nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.6
            links.new(tex.outputs['Color'],normal.inputs['Color']);links.new(normal.outputs['Normal'],shader.inputs['Normal'])
        else:
            channels=nodes.new('ShaderNodeSeparateColor');links.new(tex.outputs['Color'],channels.inputs['Color'])
            links.new(channels.outputs['Red'],shader.inputs['Metallic'])
            invert=nodes.new('ShaderNodeMath');invert.operation='SUBTRACT';invert.inputs[0].default_value=1
            links.new(tex.outputs['Alpha'],invert.inputs[1]);links.new(invert.outputs[0],shader.inputs['Roughness'])
    cache[key]=mat;return mat

def place(name, at=(0,0,0), yaw=0):
    old=set(bpy.context.scene.objects)
    bpy.ops.import_scene.fbx(filepath=os.path.join(folder,name+'.fbx'),use_anim=False)
    created=set(bpy.context.scene.objects)-old
    survivors=[]
    for obj in list(created):
        if obj.type=='MESH' and ('_LOD1' in obj.name or '_LOD2' in obj.name):bpy.data.objects.remove(obj,do_unlink=True)
        else:survivors.append(obj)
    for obj in survivors:
        if obj.parent is None:
            obj.location+=Vector((at[0],at[2],at[1]));obj.rotation_euler.z-=math.radians(yaw)
        if obj.type=='MESH':
            for slot in obj.material_slots:
                if slot.material:slot.material=material(slot.material.name.split('.')[0])

for x in range(-3,3):
    for z in range(-2,3):place('FloorTerrazzo',(x*4,0,z*4))
place('Fountain',(0,0,0));place('Escalator',(-7,0,-3))
for x in (-2,2,6):place('StorefrontGlazed' if x!=2 else 'StorefrontBoarded',(x,0,7),180)
for x in (-4,4,8):place('Column',(x,0,7))
place('Kiosk',(6,0,-3));place('MallDirectory',(4,0,2),180)
place('Planter',(-3.6,0,-2.5));place('Overgrowth',(-3.6,.68,-2.5))
place('FoodCourtCounter',(7,0,3),180);place('FoodCourtTable',(9,0,-7))
place('CinemaMarquee',(-4,2.8,7),180);place('DebrisPile',(-5,0,3))
place('FloorTerrazzoCracked',(-4,.005,-8));place('LeafScatter',(3,.007,-5))
world=bpy.data.worlds.new('StudioOvercast');bpy.context.scene.world=world;world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.14,.17,.17,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.45
for name,at,energy,size in [('LargeSkylight',(-2,3,13),1600,14),('WarmFront',(5,-8,8),900,10)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=energy;data.shape='DISK';data.size=size
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);obj.location=at
    obj.rotation_euler=(Vector((0,1,0))-obj.location).to_track_quat('-Z','Y').to_euler()
data=bpy.data.cameras.new('ArtReview');cam=bpy.data.objects.new('ArtReview',data)
bpy.context.collection.objects.link(cam);cam.location=(17,-23,15)
cam.rotation_euler=(Vector((0,1,1.5))-cam.location).to_track_quat('-Z','Y').to_euler();data.lens=38
scene=bpy.context.scene;scene.camera=cam;scene.render.engine='CYCLES';scene.cycles.samples=24
scene.cycles.use_denoising=True;scene.render.resolution_x=1600;scene.render.resolution_y=1100
scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.render.filepath=output
scene.view_settings.view_transform='AgX';bpy.ops.render.render(write_still=True)
print('MERIDIAN_PREVIEW',output)
