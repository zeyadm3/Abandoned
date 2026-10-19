"""ABANDONED: eight original sculpted horror rigs and five animation takes.
Run Blender --background --python Tools/Blender/threats.py -- /absolute/repo
All geometry, palettes, texture noise and keyframes are original procedural content.
"""
import bpy, math, os, sys, random
from mathutils import Vector

# Blender does not put --python's directory on the import path consistently across launch methods.
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from threats_sculpt import ThreatSculpt
from threats_humanoids import blind, stalker, collector, hunter
from threats_aberrations import weight, thing, crawlers, last

REPO = sys.argv[sys.argv.index('--') + 1] if '--' in sys.argv else os.path.abspath(os.path.join(os.path.dirname(__file__), '../..'))
OUT = os.path.join(REPO, 'Game/Assets/_Project/Art/Custom/Threats')
os.makedirs(OUT, exist_ok=True)
PALETTE = {'Ash':(0.43,0.47,0.43,1), 'Charcoal':(0.105,0.135,0.14,1), 'Canvas':(0.31,0.29,0.23,1), 'Bone':(0.56,0.58,0.49,1), 'Iron':(0.21,0.24,0.25,1), 'Eyes':(0.39,0.50,0.41,1), 'Rust':(0.30,0.18,0.13,1)}

# The same low-contrast grime map keeps all creatures inside one restrained material language.
rng = random.Random(8671)
image = bpy.data.images.new('ThreatGrime', width=256, height=256)
pixels=[]
for y in range(256):
    for x in range(256):
        n = rng.random() * 0.16 + math.sin(x*.043)*math.sin(y*.065)*.07
        v = .63 + n
        pixels.extend((v*.91,v,v*.92,1))
image.pixels = pixels
image.filepath_raw = os.path.join(OUT, 'ThreatGrime.png')
image.file_format = 'PNG'; image.save()

materials={}
for name,color in PALETTE.items():
    m=bpy.data.materials.new('THREAT_'+name);m.diffuse_color=color;m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=color
    bs.inputs['Roughness'].default_value=.88
    noise=m.node_tree.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=23
    bump=m.node_tree.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.18
    m.node_tree.links.new(noise.outputs['Fac'],bump.inputs['Height']);m.node_tree.links.new(bump.outputs['Normal'],bs.inputs['Normal'])
    materials[name]=m

rig=None; parts=[]; bones={}
def bone(name,head,tail,parent=None):
    eb=rig.data.edit_bones.new(name);eb.head=head;eb.tail=tail
    if parent:eb.parent=rig.data.edit_bones[parent]
    bones[name]=(Vector(head),Vector(tail));return eb

def rig_begin(name,height,width,leg,arm,body_y=0):
    global rig,parts,bones
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    for action in list(bpy.data.actions):bpy.data.actions.remove(action)
    parts=[];bones={}
    data=bpy.data.armatures.new(name+'_Skeleton');rig=bpy.data.objects.new(name+'_Rig',data);bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
    bone('root',(0,0,0),(0,0,.25))
    bone('spine',(0,body_y,leg),(0,body_y,height*.77),'root')
    bone('neck',(0,body_y,height*.77),(0,body_y,height*.89),'spine')
    bone('head',(0,body_y,height*.89),(0,body_y,height),'neck')
    for suffix,sign in [('L',-1),('R',1)]:
        shoulder=(sign*width,body_y,height*.76);elbow=(sign*(width+.04),body_y-.06,height*.76-arm*.5);hand=(sign*(width+.08),body_y-.12,height*.76-arm)
        bone('arm'+suffix,shoulder,elbow,'spine');bone('forearm'+suffix,elbow,hand,'arm'+suffix)
        hip=(sign*width*.43,body_y,leg);knee=(sign*width*.46,body_y+.03,leg*.48);foot=(sign*width*.50,body_y-.12,.1)
        bone('leg'+suffix,hip,knee,'root');bone('shin'+suffix,knee,foot,'leg'+suffix)
    bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)

def finish(name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();mesh=bpy.context.object;mesh.name=name+'_SkinnedMesh'
    mesh.parent=rig;mod=mesh.modifiers.new('AbandonedSkeleton','ARMATURE');mod.object=rig
    for p in rig.pose.bones:p.rotation_mode='XYZ'
    for anim,length in [('Idle',72),('Walk',36),('Chase',24),('Attack',30),('Special',60)]:
        action=bpy.data.actions.new(anim);action.use_fake_user=True;rig.animation_data_create();rig.animation_data.action=action
        # Include the exact final pose so loops close and an attack returns fully to rest.
        frames = list(range(1, length + 1, 3))
        if frames[-1] != length: frames.append(length)
        for f in frames:
            u=(f-1)/(length-1);phase=u*math.pi*2
            for p in rig.pose.bones:p.rotation_euler=(0,0,0);p.location=(0,0,0)
            sway=math.sin(phase)
            if anim=='Idle':
                rig.pose.bones['spine'].rotation_euler[1]=sway*.028
                rig.pose.bones['head'].rotation_euler[2]=math.sin(phase*2)*.065
                rig.pose.bones['forearmL'].rotation_euler[0]=max(0,math.sin(phase*7))*.08
            elif anim in ['Walk','Chase']:
                fast=anim=='Chase';stride=.62 if fast else .32
                for side,sign in [('L',1),('R',-1)]:
                    rig.pose.bones['leg'+side].rotation_euler[0]=sway*stride*sign
                    rig.pose.bones['shin'+side].rotation_euler[0]=max(0,-sway*sign)*stride*.9
                    rig.pose.bones['arm'+side].rotation_euler[0]=-sway*stride*.5*sign-(.35 if fast else .08)
                    rig.pose.bones['forearm'+side].rotation_euler[2]=math.sin(phase*4+sign)*.1
                rig.pose.bones['spine'].rotation_euler[0]=.15 if fast else .025
                rig.pose.bones['root'].location[2]=abs(sway)*(.055 if fast else .025)
                rig.pose.bones['head'].rotation_euler[2]=math.sin(phase*5)*(.12 if name in ['BlindOne','Thing'] else .04)
            elif anim=='Attack':
                lunge=math.sin(math.pi*u)
                rig.pose.bones['spine'].rotation_euler[0]=lunge*.54
                for side in ['L','R']:
                    rig.pose.bones['arm'+side].rotation_euler[0]=-lunge*1.6
                    rig.pose.bones['forearm'+side].rotation_euler[0]=-lunge*.5
                rig.pose.bones['head'].rotation_euler[1]=math.sin(phase*3)*.23
            else:
                if name=='BlindOne':rig.pose.bones['head'].rotation_euler[2]=sway*.55;rig.pose.bones['neck'].rotation_euler[0]=.25
                elif name=='Stalker':rig.pose.bones['head'].rotation_euler[1]=.3;rig.pose.bones['forearmR'].rotation_euler[2]=math.sin(phase*9)*.06
                elif name=='Collector':
                    for side in ['L','R']:rig.pose.bones['arm'+side].rotation_euler[0]=-1.1;rig.pose.bones['forearm'+side].rotation_euler[0]=-.7
                    rig.pose.bones['spine'].rotation_euler[0]=.3+abs(sway)*.08
                elif name=='Hunter':rig.pose.bones['root'].rotation_euler[0]=1.05;rig.pose.bones['head'].rotation_euler[2]=sway*.06
                elif name=='Weight':rig.pose.bones['root'].location[2]=-.28*abs(sway);rig.pose.bones['spine'].rotation_euler[1]=sway*.11
                elif name=='Thing':rig.pose.bones['head'].rotation_euler[1]=sway*1.2;rig.pose.bones['neck'].rotation_euler[2]=math.sin(phase*3)*.17
                elif name=='Crawlers':rig.pose.bones['root'].rotation_euler[2]=sway*.35;rig.pose.bones['armL'].rotation_euler[0]=math.sin(phase*4)*.4;rig.pose.bones['armR'].rotation_euler[0]=-math.sin(phase*4)*.4
                else:rig.pose.bones['spine'].rotation_euler[0]=-.25*abs(sway);rig.pose.bones['head'].rotation_euler[2]=sway*.3
            for p in rig.pose.bones:p.keyframe_insert(data_path='rotation_euler',frame=f);p.keyframe_insert(data_path='location',frame=f)
        for fc in action.fcurves:
            for key in fc.keyframe_points:key.interpolation='LINEAR'
    rig.animation_data.action=bpy.data.actions['Idle'];bpy.context.scene.frame_start=1;bpy.context.scene.frame_end=72;bpy.context.scene.render.fps=30
    bpy.context.scene.frame_set(1);bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);mesh.select_set(True);bpy.context.view_layer.objects.active=rig
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0.0,path_mode='AUTO')
    print('EXPORTED '+name,flush=True)

MODELS = [
    ('BlindOne', blind, (2.65, .39, 1.02, 1.40, 0)),
    ('Stalker', stalker, (3.1, .26, .75, 1.92, 0)),
    ('Collector', collector, (1.5, .36, .43, .85, -.04)),
    ('Hunter', hunter, (2.75, .70, .92, 1.38, 0)),
    ('Weight', weight, (4.9, 1.12, 2.9, 2.25, .25)),
    ('Thing', thing, (2.35, .37, .94, 1.25, 0)),
    ('Crawlers', crawlers, (.88, .40, .25, .58, .22)),
    ('LastHunter', last, (3.9, .90, 1.12, 2.68, 0)),
]
for name, build, proportions in MODELS:
    rig_begin(name, *proportions)
    build(ThreatSculpt(materials, parts, bones))
    finish(name)
