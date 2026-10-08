"""Generate only the original realistic Meridian kit; legacy HQ assets are untouched.

Run Blender --background --python Tools/Blender/meridian.py -- /absolute/repo.
Optional second argument: textures, meshes, or all (default). Use --force-textures
as a third argument to regenerate existing PNG contents while preserving metadata.
"""
import bpy, os, sys, json
sys.dont_write_bytecode=True

sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
import meridian_textures as textures
import meridian_core as core
import meridian_surfaces, meridian_architecture, meridian_props, meridian_decay

args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
root=args[0] if args else os.path.abspath(os.path.join(os.path.dirname(__file__),'../..'))
mode=args[1] if len(args)>1 else 'all'
output=os.path.join(root,'Game/Assets/_Project/Art/Custom/Environment/Meridian')
os.makedirs(output,exist_ok=True)

if mode in ('all','textures'):textures.generate(output,'--force-textures' in args)
if mode in ('all','meshes'):
    core.initialize(output,textures.SETS)
    meridian_surfaces.generate(); meridian_architecture.generate()
    meridian_props.generate(); meridian_decay.generate()
    meridian_architecture.fractured_flights()
if mode=='flight-fractures':
    core.initialize(output,textures.SETS)
    meridian_architecture.fractured_flights()

manifest={'version':'0.13.0','author':'Original ABANDONED art',
    'coordinateMapping':'Unity (x,y,z) = Blender (x,z,y); metres; +Z front/climb',
    'pivot':'Floor walking-top centre; flight foot centre; other module base centre',
    'lodHierarchy':'LOD0/1/2 children; <Model>_LOD0/1/2 renderers. Diffuser separate.',
    'fractureHierarchy':'Floor*Fractured:9 sibling chunks; flight *Fractured:10 siblings; centroid pivots',
    'maskPacking':{'r':'metallic','g':'occlusion','b':'unused','a':'smoothness'},
    'textures':{key:{'size':size,'slot':'MALL_'+key,'paths':[
        'Textures/MALL_'+key+'_'+suffix+'.png' for suffix in ('BaseColor','Normal','Mask')]}
        for key,size in textures.SETS.items()},
    'models':sorted(f[:-4] for f in os.listdir(output) if f.endswith('.fbx'))}
with open(os.path.join(output,'MeridianManifest.json'),'w') as f:json.dump(manifest,f,indent=2)
print('MERIDIAN_COMPLETE',len(manifest['models']),'models',len(textures.SETS),'original PBR sets')
