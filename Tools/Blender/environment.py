"""ABANDONED original modular environment kit. Blender 4.3+, no external inputs.
Run: blender --background --python Tools/Blender/environment.py -- /absolute/repo
Unity metres: authored on 4 m grid, Y up; all origins are floor/base centre except
floor tiles (walking surface at Y=0) and flights (foot at zero, +Z climb 8 m).
Textures are deterministic, original noise/grain surfaces, written beside meshes.
"""
import bpy, math, os, random, sys
from mathutils import Vector
args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
ROOT = args[0] if args else os.path.abspath(os.path.join(os.path.dirname(__file__), '../..'))
OUT = os.path.join(ROOT, 'Game/Assets/_Project/Art/Custom/Environment')
os.makedirs(os.path.join(OUT, 'Textures'), exist_ok=True)
random.seed(1200)
PALETTE = {
 'Concrete': (.35,.36,.34), 'Plaster': (.49,.49,.43), 'Tile': (.43,.47,.45),
 'Trim': (.18,.24,.23), 'Rust': (.30,.19,.13), 'Metal': (.25,.28,.28),
 'Rubber': (.07,.075,.07), 'Wood': (.30,.24,.16), 'Glass': (.22,.34,.34),
 'Ceiling': (.49,.47,.40), 'Paper': (.61,.57,.43), 'Cloth': (.26,.28,.24),
 'Mould': (.11,.17,.10), 'Stain': (.18,.18,.12), 'Water': (.12,.20,.21),
 'Dirt': (.22,.23,.16), 'LightAmber': (.68,.50,.27), 'LightRed': (.51,.055,.025),
 'Chalk': (.64,.61,.48), 'Black': (.04,.045,.04), 'Brass': (.42,.34,.16),
}
MATS = {}
for key, color in PALETTE.items():
    image = bpy.data.images.new('ENV_' + key, width=128, height=128)
    pixels = []
    rng = random.Random('abandoned-' + key)
    for yy in range(128):
        for xx in range(128):
            grain = rng.uniform(-.05, .05)
            macro = .035 * math.sin(xx*.13 + math.sin(yy*.09)*2) + .025*math.sin(yy*.17)
            streak = -.035 if key in ('Rust','Plaster','Ceiling','Stain') and (xx//5)%9 == 0 else 0
            tile = -.07 if key=='Tile' and (xx%32<2 or yy%32<2) else 0
            fleck = -.13 if rng.random()<.024 and key not in ('Glass','Water','LightAmber','LightRed') else 0
            pixels.extend([max(.01, min(1, c+grain+macro+streak+tile+fleck)) for c in color]+[1])
    image.pixels.foreach_set(pixels)
    image.filepath_raw = os.path.join(OUT, 'Textures', 'ENV_' + key + '.png')
    image.file_format = 'PNG'; image.save()
    mat = bpy.data.materials.new('ENV_' + key); mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Base Color'].default_value = (*color,1)
    shader.inputs['Roughness'].default_value = .88 if key not in ('Glass','Water','Metal','Brass') else .26
    shader.inputs['Metallic'].default_value = .7 if key in ('Metal','Brass','Rust') else 0
    tex = mat.node_tree.nodes.new('ShaderNodeTexImage'); tex.image = image
    mat.node_tree.links.new(tex.outputs['Color'], shader.inputs['Base Color'])
    MATS[key] = mat

OBJECTS = []
def xyz(p): return (p[0], -p[2], p[1])
def material(obj, key): obj.data.materials.append(MATS[key]); OBJECTS.append(obj); return obj
def box(name, p, s, key='Concrete', bevel=.012, rot=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=xyz(p))
    obj = bpy.context.object; obj.name = name; obj.dimensions = (s[0],s[2],s[1]); obj.rotation_euler.z = -math.radians(rot)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel and min(s)>.02:
        mod=obj.modifiers.new('WornEdges','BEVEL'); mod.width=min(bevel,min(s)*.2); mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return material(obj,key)
def ellipsoid(name,p,s,key='Cloth',sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=xyz(p))
    obj=bpy.context.object;obj.name=name;obj.scale=(s[0],s[2],s[1]); bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return material(obj,key)
def cylinder(name,p,r,h,key='Metal',verts=12):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=h,location=xyz(p))
    obj=bpy.context.object;obj.name=name;return material(obj,key)
def rod(name,a,b,r=.025,key='Metal',verts=8):
    aa,bb=Vector(xyz(a)),Vector(xyz(b));delta=bb-aa
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=delta.length,location=(aa+bb)*.5)
    obj=bpy.context.object; obj.name=name;obj.rotation_euler=delta.to_track_quat('Z','Y').to_euler();return material(obj,key)
def mesh(name, verts, faces, key):
    data=bpy.data.meshes.new(name);data.from_pydata([xyz(v) for v in verts],[],faces);data.update()
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj);return material(obj,key)
def ring(name,p,outer,inner,height,key='Concrete',segments=20):
    vs=[]
    for y in (p[1]-height/2,p[1]+height/2):
        for r in (outer,inner):
            vs.extend([(p[0]+math.cos(i*math.tau/segments)*r,y,p[2]+math.sin(i*math.tau/segments)*r) for i in range(segments)])
    fs=[]
    for i in range(segments):
        j=(i+1)%segments;n=segments
        fs += [(i,j,2*n+j,2*n+i),(n+j,n+i,3*n+i,3*n+j),(2*n+i,2*n+j,3*n+j,3*n+i),(j,i,n+i,n+j)]
    return mesh(name,vs,fs,key)
def irregular(name,p,s,key,seed):
    rng=random.Random(seed);verts=[(p[0],p[1],p[2])]
    for i in range(18):
        a=i*math.tau/18;r=rng.uniform(.58,1)
        verts.append((p[0]+math.cos(a)*s[0]*r,p[1]+rng.uniform(0,.002),p[2]+math.sin(a)*s[1]*r))
    return mesh(name,verts,[(0,i+1,(i+1)%18+1) for i in range(18)],key)
def letters(text,p,size,key='Chalk'):
    bpy.ops.object.text_add(location=xyz(p)); obj=bpy.context.object;obj.name='Inscription'
    # Blender text lies in XY; turn its front toward Unity +Z.
    obj.rotation_euler=(math.pi/2,0,0);obj.data.body=text;obj.data.size=size;obj.data.align_x='CENTER';obj.data.align_y='CENTER';obj.data.extrude=.001
    bpy.ops.object.convert(target='MESH');return material(bpy.context.object,key)
def begin():
    global OBJECTS
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);OBJECTS=[]
def export(name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in OBJECTS:o.select_set(True)
    bpy.context.view_layer.objects.active=OBJECTS[0]
    bpy.ops.object.join();obj=bpy.context.object;obj.name=name
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.15,island_margin=.02);bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,path_mode='RELATIVE',bake_anim=False)
    print('EXPORTED '+name)

# Modular walls, finish layers and openings; trims project 3 cm on each face.
for name,hole in [('WallSolid',None),('WallDoor',(1.4,0,2.3)),('WallWide',(3.5,0,3.4)),('WallWindow',(1.4,1,1.3))]:
    begin();h=3.82
    if hole:
        w,b,hh=hole;side=(4-w)*.5
        for sign in (-1,1):box('Pier',(sign*(w*.5+side*.5),h*.5,0),(side,h,.20),'Plaster')
        if b:box('Sill',(0,b*.5,0),(w,b,.20),'Plaster')
        box('Header',(0,(b+hh+h)*.5,0),(w,h-b-hh,.20),'Plaster')
        for sign in (-1,1):box('DoorJamb',(sign*(w*.5+.015),(b+hh)*.5,0),(.06,hh,.27),'Trim')
        box('Lintel',(0,b+hh+.03,0),(w+.12,.07,.27),'Trim')
    else:box('PlasterCore',(0,h*.5,0),(4,h,.20),'Plaster')
    for sign in (-1,1):
        for x,ww in ([(-1.5,1),(1.5,1)] if hole and hole[1]==0 else [(0,4)]):
            box('Skirting',(x,.08,sign*.115),(ww,.14,.04),'Trim')
            box('Dado',(x,.96,sign*.115),(ww,.05,.045),'Trim')
    export(name)
begin()
for x in (-1,1):box('StonePier',(x*1.9,1.85,0),(.2,3.7,.32),'Tile');box('ShopMullion',(x*1.77,1.68,.05),(.06,3.35,.14),'Metal')
box('Fascia',(0,3.58,0),(3.8,.48,.36),'Trim');box('FasciaRim',(0,3.84,.02),(4,.035,.41),'Brass');export('Storefront')
for name,key in [('FloorTile','Tile'),('FloorConcrete','Concrete'),('FloorDamaged','Concrete')]:
    begin();box('StructuralFinish',(0,-.09,0),(4,.18,4),key)
    if name=='FloorTile':
        for x in (-1.99,1.99):box('Border',(x,.002,0),(.025,.004,4),'Trim',0)
    if name=='FloorDamaged':
        for i in range(7):rod('Fracture',(-1.8+i*.52,.004,math.sin(i)*.22),( -1.28+i*.52,.004,math.sin(i+1)*.22),.012,'Black')
    export(name)
for name,broken in [('CeilingGrid',False),('CeilingSagging',True)]:
    begin()
    for i in range(5):
        box('Tbar',(-2+i,0,0),(.028,.045,4),'Metal');box('Tbar',(0,0,-2+i),(4,.045,.028),'Metal')
    for x in range(4):
        for z in range(4):
            if broken and (x,z) in ((0,0),(1,0),(2,2)):continue
            panel=box('MineralPanel',(-1.5+x,-.025-(.16 if broken and x==2 else 0),-1.5+z),(.95,.04,.95),'Ceiling',0)
            if broken and x==2:panel.rotation_euler.x=.11
    if broken:
        for i in range(3):rod('HangingWire',(.25+i*.2,0,.3),(.3+i*.19,-.65,.4),.009,'Rust')
    export(name)
begin();box('Core',(0,1.91,0),(.65,3.82,.65),'Concrete');box('Plinth',(0,.12,0),(.86,.24,.86),'Tile');box('Capital',(0,3.71,0),(.83,.20,.83),'Trim')
for z in (-.328,.328):box('Inset',(0,1.95,z),(.45,3.23,.018),'Plaster')
export('Pillar')
begin();box('Base',(0,.1,0),(4,.2,.12),'Trim')
for x in (-1.98,-1,0,1,1.98):box('Post',(x,.55,0),(.07,1.1,.10),'Metal')
box('Handrail',(0,1.08,0),(4,.09,.16),'Rubber');box('Glass',(0,.58,0),(3.86,.84,.035),'Glass');export('AtriumRailing')
for name,width,escalator in [('ServiceStairs',3,False),('Escalator',2.4,True)]:
    begin()
    for i in range(20):
        y=(i+1)*.2
        box('Tread',(0,y-.06,(i+.5)*.4),(width,.12,.4),'Metal' if escalator else 'Concrete')
        box('SafetyEdge',(0,y+.002,i*.4+.025),(width,.014,.04),'Brass')
        if escalator:
            for j in range(7):box('Groove',(-width/2+.14+j*(width-.28)/6,y+.007,(i+.5)*.4),(.01,.012,.36),'Black',0)
    for sign in (-1,1):
        x=sign*(width*.5+.08)
        rod('Handrail',(x,1.05,0),(x,5.05,8),.067,'Rubber',12)
        for i in range(9):
            z=i;rod('Baluster',(x,z*.5,z),(x,z*.5+.98,z),.035,'Metal')
        if escalator:
            mesh('GlassSide',[(x,0,0),(x,4,8),(x,5,8),(x,1,0)],[(0,1,2,3)],'Glass')
            rod('Skirt',(x,.05,0),(x,4.05,8),.11,'Metal')
    export(name)
begin()
for y in [i*.12+.06 for i in range(28)]:box('CorrugatedSlat',(0,y,0),(3.35,.118,.07),'Metal',.008);box('Rib',(0,y+.043,.05),(3.35,.015,.02),'Rust',0)
box('BottomRail',(0,.045,0),(3.4,.09,.14),'Trim');box('LockPlate',(0,.21,.05),(.18,.21,.03),'Rust');export('RollerShutter')
begin();box('SteelLeaf',(0,1.1,0),(1.28,2.2,.06),'Trim')
for y in (.44,1.40):box('RecessedPanel',(0,y,.037),(1.0,.55,.015),'Metal')
rod('PushBar',(-.44,1,.10),(.44,1,.10),.025,'Metal');box('Hinges',(-.63,1.1,0),(.045,1.7,.1),'Rust');export('ServiceDoor')

# Retail silhouettes, distinct from salvage loot models.
begin();box('KioskBase',(0,.55,0),(2.4,1.1,1.8),'Trim');box('CounterTop',(0,1.14,0),(2.65,.12,2.02),'Wood')
for x in (-1.18,1.18):rod('AwningPost',(x,1.2,.76),(x,2.62,.76),.045,'Metal')
box('Awning',(0,2.65,0),(2.7,.18,2.0),'Trim');box('Till',( .62,1.35,.42),(.4,.3,.38),'Black');export('Kiosk')
begin()
for x in (-.8,.8):box('Upright',(x,1.06,0),(.065,2.12,.48),'Rust')
for y in (.15,.65,1.15,1.65,2.10):box('Shelf',(0,y,0),(1.72,.05,.52),'Metal')
for i in range(6):box('DiscardedCarton',(-.63+(i%3)*.48,.86+(i//3)*.52,.03),(.32,.31,.30),'Paper')
export('RetailShelf')
begin();box('Counter',(0,.51,0),(2.0,1.02,.70),'Trim');box('Worktop',(0,1.055,0),(2.12,.09,.82),'Wood')
for x in (-.50,.50):box('CabinetDoor',(x,.55,.358),(.94,.78,.028),'Wood');rod('Handle',(x-.18,.86,.40),(x+.18,.86,.40),.014,'Metal')
export('Counter')
begin()
for x in (-.75,.75):box('Leg',(x,.23,0),(.12,.46,.55),'Metal')
for z in (-.22,-.075,.075,.22):box('SeatSlat',(0,.48,z),(2.0,.06,.115),'Wood')
for y in (.70,.89):box('BackSlat',(0,y,-.26),(2.0,.12,.07),'Wood')
rod('ArmL',(-.88,.48,-.24),(-.88,.78,.24),.03,'Metal');rod('ArmR',(.88,.48,-.24),(.88,.78,.24),.03,'Metal');export('Bench')
begin();ring('EmptyBasin',(0,.33,0),2.25,1.97,.52,'Tile',24);cylinder('BasinFloor',(0,.055,0),2,.1,'Concrete',24);cylinder('Pedestal',(0,.62,0),.45,1.12,'Concrete');ring('UpperBowl',(0,1.27,0),1.1,.9,.18,'Concrete');cylinder('BrokenSpout',(0,1.55,0),.1,.55,'Rust');irregular('StandingWater',(0,.11,0),(1.94,1.94),'Water',34);export('Fountain')
for name,pose in [('Mannequin',False),('MannequinWrong',True)]:
    begin();cylinder('Stand',(0,.04,0),.28,.08,'Metal');rod('Pole',(0,.04,0),(0,.8,0),.025,'Metal')
    for sign in (-1,1):
        rod('Shin',(sign*.12,.10,0),(sign*.13,.54,.025),.075,'Plaster');rod('Thigh',(sign*.13,.54,.025),(sign*.12,1.00,0),.11,'Plaster')
    ellipsoid('Pelvis',(0,1.0,0),(.22,.17,.13),'Plaster',2);ellipsoid('Torso',(0,1.29,0),(.25,.29,.14),'Plaster',2);rod('Neck',(0,1.5,0),(0,1.63,0),.055,'Plaster')
    head=ellipsoid('BlankFace',(0,1.77,0),(.12,.17,.105),'Plaster',2);head.rotation_euler.y=.42 if pose else 0
    for sign in (-1,1):
        shoulder=(sign*.26,1.45,0);elbow=(sign*.32,1.1,.02);hand=(sign*.31,.82,.10)
        if pose and sign==1:elbow=(.45,1.71,.05);hand=(.17,1.97,.10)
        rod('Arm',shoulder,elbow,.055,'Plaster');rod('Forearm',elbow,hand,.045,'Plaster');ellipsoid('Hand',hand,(.04,.09,.035),'Plaster')
    export(name)
for name,text in [('SignExit','EXIT'),('SignDontGoUp',"DON'T GO UP"),('SignItHearsYou','IT HEARS YOU'),('SignQuarantine','KEEP OUT\nISOLATION'),('SignBrandA','VANTA / GOODS'),('SignBrandB','NORTHGLASS'),('SignBrandC','LATE SHOW'),('SignDepot','ASHLINE SALVAGE')]:
    begin();box('Signboard',(0,.3,0),(2.3,.6,.05),'Trim');letters(text,(0,.3,.032),.18,'LightRed' if name=='SignExit' else 'Chalk');export(name)
begin();box('NoticeBacking',(0,.5,0),(.65,1,.014),'Paper',0);letters('MISSING',(0,.88,.012),.09,'Black');ellipsoid('FadedPortrait',(0,.57,.012),(.12,.19,.008),'Stain',2);letters('LAST SEEN HERE',(0,.16,.014),.04,'Black');export('MissingPoster')
begin()
for x in (-.95,.95):
    for z in (-.24,.24):rod('Trestle',(x,.05,z),(x,1.2,0),.04,'Rust')
for y in (.64,1.02):box('Plank',(0,y,0),(2.4,.22,.06),'Wood');
for x in (-.9,-.3,.3,.9):box('WarningStripe',(x,1.025,.036),(.17,.19,.008),'LightAmber',0,rot=0)
export('Barricade')
begin();ellipsoid('PackBody',(0,.29,0),(.26,.31,.17),'Cloth',2);box('FrontPocket',(0,.22,.15),(.34,.28,.06),'Trim');rod('ShoulderStrap',(-.14,.5,-.12),(-.14,.1,-.12),.025,'Rubber');rod('ShoulderStrap',(.14,.5,-.12),(.14,.1,-.12),.025,'Rubber');export('AbandonedBackpack')
begin();box('Suitcase',(0,.26,0),(.72,.48,.24),'Cloth');rod('Handle',(-.14,.53,0),(.14,.53,0),.025,'Metal');
for x in (-.33,.33):ellipsoid('Wheel',(x,.03,0),(.045,.045,.045),'Rubber')
export('Suitcase')
begin();box('LockerBody',(0,1.05,0),(1.8,2.1,.5),'Trim')
for x in (-.6,0,.6):
    box('LockerDoor',(x,1.04,.26),(.55,1.96,.025),'Metal')
    for y in (1.65,1.73,1.81):box('Vent',(x,y,.277),(.35,.014,.007),'Black',0)
    rod('Latch',(x+.15,.9,.29),(x+.15,1.04,.29),.015,'Rust')
export('DepotLockers')
begin();box('Desk',(0,.76,0),(2,.12,.9),'Wood')
for x in (-.85,.85):box('DrawerPedestal',(x,.35,0),(.28,.7,.75),'Trim')
box('CRTBody',(-.35,1.03,0),(.53,.44,.47),'Trim');box('GlassScreen',(-.35,1.03,.245),(.42,.3,.016),'Glass');box('Keyboard',(-.3,.85,.4),(.56,.05,.18),'Black');export('SecurityDesk')
begin();box('Table',(0,.78,0),(2.4,.12,1.0),'Wood')
for x in (-1,1):box('Support',(x,.38,0),(.16,.76,.8),'Metal')
for z in (-.75,.75):box('Bench',(0,.46,z),(2.4,.1,.36),'Wood');
export('FoodCourtTable')
begin();box('Seat',(0,.48,0),(.56,.16,.57),'Cloth');box('SeatBack',(0,.89,-.23),(.58,.75,.16),'Cloth')
for x in (-.35,.35):box('Armrest',(x,.65,0),(.10,.12,.62),'Rubber');box('Frame',(x,.27,0),(.055,.53,.42),'Metal')
export('CinemaSeat')
begin();box('Housing',(0,0,0),(1.15,.10,.25),'Metal');box('Diffuser',(0,-.065,0),(1.02,.045,.17),'LightAmber');
for x in (-.53,.53):box('EndCap',(x,-.035,0),(.075,.10,.24),'Rubber')
export('FluorescentFixture')
begin();box('CagedHousing',(0,0,0),(.28,.18,.20),'Trim');box('RedLens',(0,-.04,.11),(.21,.1,.025),'LightRed')
for x in (-.09,0,.09):rod('Guard',(x,-.10,.13),(x,.04,.13),.008,'Metal')
export('EmergencyFixture')

# Thin, collision-free decay dressing authored as geometry to avoid decal/package dependency.
for name,key,shape in [('WaterStain','Stain',(1.0,.55)),('MouldPatch','Mould',(.7,.45)),('Puddle','Water',(1.15,.7)),('FloodWater','Water',(1.99,1.99))]:
    begin();irregular('Patch',(0,.003,0),shape,key,name)
    if name=='MouldPatch':
        for i in range(10):ellipsoid('Bloom',(math.sin(i*4)*.55,.012,math.cos(i*3)*.35),(.12,.009,.1),'Mould')
    export(name)
begin()
for i in range(9):rod('Crack',(-1.5+i*.36,.004,math.sin(i*2)*.18),(-1.14+i*.36,.004,math.sin((i+1)*2)*.18),.014,'Black')
for i in (2,4,6):rod('Branch',(-1.5+i*.36,.005,math.sin(i*2)*.18),(-1.4+i*.36,.005,.55),.009,'Black')
export('FloorCrack')
begin()
for i in range(12):
    rng=random.Random(44+i);x=rng.uniform(-.8,.8);z=rng.uniform(-.5,.5)
    ellipsoid('Rubble',(x,rng.uniform(.025,.13),z),(rng.uniform(.07,.23),rng.uniform(.04,.14),rng.uniform(.08,.21)),'Concrete')
for i in range(3):box('CeilingScrap',(-.3+i*.3,.025,.25),(.4,.035,.3),'Ceiling',0,rot=i*35)
export('DebrisPile')
begin()
for i in range(16):
    rng=random.Random(i*41);x=rng.uniform(-.7,.7);z=rng.uniform(-.4,.4)
    mesh('Shard',[(x,.005,z),(x+rng.uniform(.06,.25),.008,z+.03),(x+.04,.006,z+rng.uniform(.07,.2))],[(0,1,2)],'Glass')
export('BrokenGlass')
begin()
for i in range(6):rod('Rebar',(-.8+i*.28,.1,-.4),(-.84+i*.28,.7,.52),.018,'Rust')
for i in range(4):rod('Tie',(-.9,.14+i*.15,-.35+i*.22),(.85,.14+i*.15,-.35+i*.22),.014,'Rust')
export('ExposedRebar')
begin()
for i in range(12):
    rng=random.Random(12+i);x=rng.uniform(-.65,.65);z=rng.uniform(-.45,.45);h=rng.uniform(.3,.9)
    rod('DeadStem',(x,0,z),(x+.08,h,z+.04),.012,'Wood')
    for j in range(3):ellipsoid('Leaf',(x+.06,h*(.4+j*.2),z+.05),(.10,.025,.05),'Mould')
export('Overgrowth')
begin();irregular('ChippedConcrete',(0,.01,0),(.75,.55),'Concrete',12)
for i in range(4):rod('ProtrudingBar',(-.4+i*.26,.02,-.3),(-.44+i*.28,.12,.35),.015,'Rust')
export('BrokenSlabEdge')
begin()
for i in range(9):rod('Sigil',(.42*math.cos(i*math.tau/9),.005,.42*math.sin(i*math.tau/9)),(.42*math.cos((i+3)*math.tau/9),.005,.42*math.sin((i+3)*math.tau/9)),.01,'Chalk')
export('StrangeSymbol')
print('Original ABANDONED environment kit generated.')
