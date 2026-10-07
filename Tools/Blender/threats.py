"""ABANDONED: eight original low-mid-poly skinned threat rigs and five animation takes.
Run Blender --background --python Tools/Blender/threats.py -- /absolute/repo
All geometry, palettes, texture noise and keyframes are original procedural content.
"""
import bpy, math, os, sys, random
from mathutils import Vector

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
def part(name,at,size,mat,bone='spine',shape='sphere',rotation=None):
    if shape=='sphere':bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,location=at)
    elif shape=='cone':bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=1,radius2=.68,depth=2,location=at)
    elif shape=='cylinder':bpy.ops.mesh.primitive_cylinder_add(vertices=10,radius=1,depth=2,location=at)
    else:bpy.ops.mesh.primitive_cube_add(size=2,location=at)
    obj=bpy.context.object;obj.name=name;obj.scale=Vector(size)/2
    if rotation:obj.rotation_euler=rotation
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    obj.data.materials.append(materials[mat]);group=obj.vertex_groups.new(name=bone);group.add(list(range(len(obj.data.vertices))),1,'REPLACE')
    # Deliberate broad facets with small highlights, not unmodified cube characters.
    if shape=='box':
        mod=obj.modifiers.new('Worn softened edges','BEVEL');mod.width=.025;mod.segments=1
        bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=mod.name)
    parts.append(obj);return obj

def beam(name,a,b,width,mat,bone,depth=None):
    a=Vector(a);b=Vector(b);direction=b-a
    obj=part(name,(a+b)*.5,(width,depth or width,direction.length),mat,bone,'cylinder')
    obj.rotation_euler=direction.to_track_quat('Z','Y').to_euler();return obj

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

def limbs(mat,armwidth,legwidth,claws=False):
    for suffix in ['L','R']:
        for b,w in [('arm'+suffix,armwidth),('forearm'+suffix,armwidth*.78),('leg'+suffix,legwidth),('shin'+suffix,legwidth*.83)]:
            beam(b+'Skin',*bones[b],w,mat,b,w*1.1)
        h=bones['forearm'+suffix][1];part('LongHand'+suffix,h,(armwidth*1.4,armwidth*1.3,.22),mat,'forearm'+suffix)
        f=bones['shin'+suffix][1];part('Foot'+suffix,f+Vector((0,-.12,-.03)),(legwidth*1.05,.4,.17),mat,'shin'+suffix,'box')
        if claws:
            for i in range(3):beam('HookedFinger',h+Vector(((i-1)*.055,-.02,-.04)),h+Vector(((i-1)*.07,-.15,-.34)),.03,'Bone','forearm'+suffix)

def torso(z,size,mat='Ash',shape='sphere',y=0):part('Torso',(0,y,z),size,mat,'spine',shape)
def skull(z,size,mat='Bone',y=-.03):part('Skull',(0,y,z),size,mat,'head')
def seam(z,width=.22,y=-.2):part('SealedFace',(0,y,z),(width,.04,.025),'Charcoal','head','box')
def blind():
    rig_begin('BlindOne',2.65,.39,1.02,1.40)
    torso(1.55,(.58,.35,.9),'Bone');torso(1.03,(.32,.29,.28),'Charcoal');skull(2.32,(.40,.36,.66))
    limbs('Bone',.15,.16,True);seam(2.18)
    for sign in [-1,1]:part('ListeningFan',(sign*.30,.01,2.38),(.16,.24,.48),'Ash','head','cone',(0,sign*.2,0))
    for i in range(6):part('ExposedRib',(0,-.18,1.23+i*.115),(.43-.025*i,.055,.04),'Charcoal','spine','box')
    for sign in [-1,1]:beam('LongNeckTendon',(sign*.09,0,1.9),(sign*.09,-.02,2.2),.045,'Ash','neck')
def stalker():
    rig_begin('Stalker',3.1,.26,.75,1.92)
    torso(1.60,(.49,.32,1.65),'Charcoal','cone');part('CoatHem',(0,0,.64),(.66,.48,.34),'Charcoal','spine','cone')
    skull(2.78,(.33,.32,.62),'Charcoal');limbs('Charcoal',.085,.11,True)
    part('ClippedEyeLine',(0,-.17,2.70),(.15,.025,.012),'Eyes','head','box')
    part('RigidCollar',(0,0,2.38),(.55,.32,.11),'Iron','neck','box')
    for sign in [-1,1]:part('DrapedShoulder',(sign*.23,0,2.23),(.22,.4,.45),'Iron','arm'+('L' if sign<0 else 'R'))
    for i in range(5):part('OldCoatFastener',(0,-.19,1.19+i*.18),(.035,.03,.04),'Iron','spine')
def collector():
    rig_begin('Collector',1.5,.36,.43,.85,body_y=-.04)
    torso(.75,(.74,.65,.82),'Canvas',y=.13);part('SaggingSack',(0,.47,.93),(.9,.69,.92),'Canvas','spine')
    skull(1.27,(.43,.4,.37),'Iron',-.31);limbs('Canvas',.145,.17,True);seam(1.24,.3,-.52)
    for i in range(5):part('StolenTags',((i-2)*.12,-.34,.74+math.sin(i)*.06),(.075,.025,.13),'Rust','spine','box')
    for sign in [-1,1]:beam('PackStrap',(sign*.22,-.27,1.04),(sign*.22,-.28,.61),.06,'Charcoal','spine')
def hunter():
    rig_begin('Hunter',2.75,.70,.92,1.38)
    torso(1.56,(1.30,.88,1.48),'Ash');part('ShoulderBeam',(0,0,2.11),(1.76,.85,.29),'Iron','spine','box')
    skull(2.41,(.47,.47,.49),'Charcoal',-.15);limbs('Ash',.38,.37,True)
    for i in range(4):part('ReinforcedRib',(0,-.48,1.14+i*.22),(1.05,.12,.13),'Iron','spine','box')
    part('AmberNarrowEyes',(0,-.40,2.43),(.22,.04,.022),'Rust','head','box')
    for sign in [-1,1]:part('StoneKnuckles',(sign*.78,-.15,.87),(.43,.40,.43),'Iron','forearm'+('L' if sign<0 else 'R'),'box')
def weight():
    rig_begin('Weight',4.9,1.12,2.9,2.25,body_y=.25)
    torso(3.86,(3.3,2.65,1.56),'Charcoal',y=.15);skull(4.39,(.72,.79,.56),'Iron',-.85)
    limbs('Iron',.34,.29,True)
    for i in range(7):part('CeilingRidge',(0,.9-i*.34,4.22),(3.35-.12*abs(i-3),.16,.50),'Ash','spine','box')
    for sign in [-1,1]:
        for i in range(3):beam('WallTendon',(sign*1.3,.8-i*.7,3.8),(sign*1.8,.8-i*.7,2.5),.17,'Canvas','spine')
    part('BlankMask',(0,-1.23,4.4),(.38,.07,.18),'Bone','head','box')
def thing():
    rig_begin('Thing',2.35,.37,.94,1.25)
    torso(1.36,(.63,.52,.94),'Iron');skull(2.04,(.62,.49,.59),'Bone',-.10);limbs('Ash',.13,.16,True)
    # A human-shaped shell with incompatible nested face planes, without gore.
    for i in range(3):part('NestedFace',(0,-.35-i*.065,2.07+i*.012),(.43-i*.075,.05,.43-i*.06),'Bone' if i%2==0 else 'Charcoal','head','box',(0,0,(i-1)*.12))
    for sign in [-1,1]:part('FoldedShoulder',(sign*.37,0,1.69),(.22,.49,.32),'Canvas','arm'+('L' if sign<0 else 'R'))
    for i in range(4):beam('LooseCable',(0,.18,1.17+i*.11),((i-1.5)*.18,.35,.68),.035,'Charcoal','spine')
def crawlers():
    rig_begin('Crawlers',.88,.40,.25,.58,body_y=.22)
    # Five creatures share one swarm rig, lowering replication cost while each silhouette remains visible.
    for n,(x,y) in enumerate([(0,0),(-.55,.22),(.58,.28),(-.34,.82),(.42,.9)]):
        b='spine' if n==0 else 'armL' if n%2 else 'armR'
        part('Crawler'+str(n),(x,y,.31),(.51,.72,.38),'Charcoal',b)
        part('CrawlerMask'+str(n),(x,y-.36,.27),(.28,.16,.21),'Bone',b,'box')
        for sign in [-1,1]:
            for j in range(3):
                a=(x+sign*.19,y-.17+j*.19,.31);k=(x+sign*.44,y-.28+j*.19,.17);f=(x+sign*.53,y-.36+j*.2,.04)
                beam('BentLeg',a,k,.055,'Ash',b);beam('NeedleFoot',k,f,.035,'Iron',b)
def last():
    rig_begin('LastHunter',3.9,.90,1.12,2.68)
    torso(2.04,(1.13,.78,2.10),'Charcoal','cone');skull(3.51,(.49,.52,.71),'Bone',-.02);limbs('Iron',.23,.28,True)
    for sign in [-1,1]:
        for i in range(3):beam('AntlerRebar',(sign*.22,0,3.70),(sign*(.40+i*.26),.2+i*.13,3.94-i*.14),.075,'Iron','head')
        part('BrokenMantle',(sign*.59,.12,2.53),(.53,.52,1.05),'Canvas','arm'+('L' if sign<0 else 'R'),'cone',(0,sign*.3,0))
    for i in range(6):part('HangingShutter',(0,-.4,1.30+i*.25),(.90-.08*i,.075,.17),'Iron','spine','box')
    seam(3.46,.20,-.29)

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

for name,build in [('BlindOne',blind),('Stalker',stalker),('Collector',collector),('Hunter',hunter),('Weight',weight),('Thing',thing),('Crawlers',crawlers),('LastHunter',last)]:
    build();finish(name)
