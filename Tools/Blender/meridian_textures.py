"""Original PBR surfaces: manufactured patterns plus physically scaled wear."""
import bpy, os, numpy as np

SETS = {'Terrazzo':2048, 'Plaster':2048, 'StorefrontTrim':2048,
        'EscalatorSteel':2048, 'Ceiling':2048, 'Tile':1024, 'Carpet':1024,
        'Concrete':1024, 'Wood':1024, 'BridgeMetal':1024, 'Block':1024,
        'Asphalt':1024, 'FacadeStone':1024, 'Rust':1024, 'FixtureTrim':512,
        'Upholstery':512, 'BoardWood':512, 'Moss':512, 'Leaves':512,
        'Graffiti':512, 'Water':512, 'FixtureGlow':512, 'Crack':512, 'Glass':512, 'Scorch':512}
COLOURS = {'Terrazzo':(.46,.49,.45), 'Plaster':(.53,.55,.48),
           'StorefrontTrim':(.20,.26,.24), 'EscalatorSteel':(.40,.43,.42),
           'Ceiling':(.54,.55,.47), 'Tile':(.32,.40,.36),
           'Carpet':(.22,.25,.23), 'Concrete':(.39,.40,.37),
           'Wood':(.37,.29,.19), 'BridgeMetal':(.22,.25,.24),
           'Block':(.45,.48,.43), 'Asphalt':(.14,.16,.15),
           'FacadeStone':(.47,.45,.37), 'Rust':(.36,.21,.12),
           'FixtureTrim':(.19,.23,.22), 'Upholstery':(.27,.16,.12),
           'BoardWood':(.33,.27,.17), 'Moss':(.16,.23,.10),
           'Leaves':(.32,.30,.12), 'Graffiti':(.58,.35,.16),
           'Water':(.13,.22,.20), 'FixtureGlow':(.70,.74,.63), 'Crack':(.025,.033,.027),
           'Glass':(.28,.36,.31), 'Scorch':(.055,.07,.045)}

def noise(rng, n, cells):
    """Periodic value noise produces seamless macro weathering without palette static."""
    grid = rng.random((cells,cells), dtype=np.float32)
    coords = np.arange(n, dtype=np.float32) * cells / n
    a = coords.astype(np.int32); f = coords - a; f = f*f*(3-2*f)
    x0, x1 = a[None,:] % cells, (a[None,:]+1) % cells
    y0, y1 = a[:,None] % cells, (a[:,None]+1) % cells
    return ((grid[y0,x0]*(1-f)[None,:]+grid[y0,x1]*f[None,:])*(1-f)[:,None]
            +(grid[y1,x0]*(1-f)[None,:]+grid[y1,x1]*f[None,:])*f[:,None])

def save(path, name, rgb, alpha, linear=False):
    n = rgb.shape[0]
    rgba = np.empty((n,n,4), dtype=np.float32)
    rgba[:,:,:3] = np.clip(rgb,0,1); rgba[:,:,3] = alpha
    image = bpy.data.images.new(name, width=n, height=n, alpha=True)
    image.colorspace_settings.name = 'Non-Color' if linear else 'sRGB'
    image.pixels.foreach_set(rgba.ravel())
    image.filepath_raw = path; image.file_format = 'PNG'; image.save()
    bpy.data.images.remove(image)

def make(key, n, folder):
    rng = np.random.default_rng(sum((i+1)*ord(c) for i,c in enumerate(key))+1300)
    yy, xx = np.mgrid[0:n,0:n].astype(np.float32)/n
    broad = noise(rng,n,8); grime = noise(rng,n,19); fine = noise(rng,n,160)
    rgb = np.ones((n,n,3),dtype=np.float32)*COLOURS[key]
    height = (fine-.5)*.001; ao = np.ones((n,n),dtype=np.float32)
    metallic = np.zeros((n,n),dtype=np.float32); smooth = np.full((n,n),.23,dtype=np.float32)
    alpha = np.ones((n,n),dtype=np.float32)
    rgb *= (1+(broad-.5)*.11+(fine-.5)*.025)[:,:,None]
    if key == 'Terrazzo':
        # Chips are elliptical aggregate of several mineral colours, not uniform noise.
        palette = ((.67,.68,.58),(.18,.24,.22),(.62,.47,.35),(.73,.73,.66),(.34,.40,.35))
        for i in range(8200):
            cx,cy = rng.integers(0,n,2); rx=int(rng.integers(2,10)); ry=int(rng.integers(2,8))
            xs=np.arange(max(0,cx-rx),min(n,cx+rx+1)); ys=np.arange(max(0,cy-ry),min(n,cy+ry+1))
            shape=((xs[None,:]-cx)/rx)**2+((ys[:,None]-cy)/ry)**2<1
            patch=rgb[np.ix_(ys,xs)]; patch[shape]=palette[i%len(palette)]
            rgb[np.ix_(ys,xs)]=patch
        smooth[:]=.52; height*=.2
        # A narrow expansion line gives every four metre section a clear edge.
        edge=(np.minimum(xx,1-xx)<.0018)|(np.minimum(yy,1-yy)<.0018)
        rgb[edge]=(.26,.22,.12); metallic[edge]=.7; smooth[edge]=.4; height[edge]=-.001
    elif key in ('Tile','Block','FacadeStone'):
        rows=8 if key=='Tile' else 10 if key=='Block' else 4
        cols=8 if key=='Tile' else 5 if key=='Block' else 4
        shifted=xx+((np.floor(yy*rows)%2)*.5/cols if key!='Tile' else 0)
        seam=(np.mod(shifted*cols,1)<.025)|(np.mod(yy*rows,1)<.028)
        rgb[seam]=(.20,.23,.20); height[seam]=-.004; ao[seam]=.6
        rgb *= (1+(grime-.5)*.16)[:,:,None]; smooth[:]=.32 if key=='Tile' else .12
    elif key in ('EscalatorSteel','StorefrontTrim','BridgeMetal','FixtureTrim'):
        brushing=np.sin(xx*n*.7+np.sin(yy*35)*.8)*.008
        rgb += brushing[:,:,None]; height=brushing*.005
        metallic[:]=.9 if key=='EscalatorSteel' else .65; smooth[:]=.55 if key=='EscalatorSteel' else .38
        if key=='StorefrontTrim':
            # Metal, rubber and brass strips share one coherent architectural trim sheet.
            brass=yy>.76; rubber=(yy>.67)&(yy<.74)
            rgb[brass]=(.50,.40,.21); rgb[rubber]=(.055,.063,.055)
            metallic[rubber]=0; smooth[rubber]=.18
        if key=='BridgeMetal':
            ridge=(np.mod((xx+yy)*18,1)<.1)&(np.mod((xx-yy)*18,1)<.55)
            rgb[ridge]*=1.14; height[ridge]=.002
        wear=(grime>.75)&(broad>.61); rgb[wear]*=(.82,.73,.57); smooth[wear]=.14
    elif key in ('Wood','BoardWood'):
        grain=np.sin(xx*math_tau(22)+np.sin(yy*13)*2+np.sin(yy*41)*.25)
        rgb *= (1+grain*.13+(grime-.5)*.16)[:,:,None]; height=grain*.0008
        seams=np.mod(xx*8,1)<.025; rgb[seams]*=.43; height[seams]=-.002; ao[seams]=.62
        for k in range(7):
            knot=np.exp(-(((xx-(k+.4)/8)*90)**2+((yy-(k*.173)%1)*30)**2))
            rgb *= (1-knot*.22)[:,:,None]
        smooth[:]=.24 if key=='Wood' else .08
    elif key in ('Carpet','Upholstery'):
        fibre=(np.sin(xx*n*1.05)*np.sin(yy*n*.8))*.06
        weave=(np.mod(xx*96,1)<.1)|(np.mod(yy*96,1)<.1)
        rgb *= (1+fibre+(grime-.5)*.18)[:,:,None]; rgb[weave]*=.9
        height=(fine-.5)*.0012; smooth[:]=.05
    elif key in ('Concrete','Plaster','Asphalt','Rust'):
        pits=fine<(.23 if key=='Concrete' else .16)
        rgb *= (1+(grime-.5)*.18)[:,:,None]; rgb[pits]*=.77
        height=(fine-.5)*(.003 if key in ('Concrete','Asphalt') else .0014); height[pits]-=.001
        ao[pits]=.7
        if key=='Rust':
            patches=np.clip((grime-.35)*2,0,1)
            rgb=rgb*(1-patches[:,:,None]) + np.array((.14,.17,.16))*patches[:,:,None]
            metallic=patches*.6; smooth[:]=.10
        if key=='Plaster':
            tide=np.exp(-((yy-.09)/.033)**2)*(broad*.7+.3)
            rgb *= (1-tide*.25)[:,:,None]
    elif key=='Ceiling':
        perforation=(np.mod(xx*78,1)<.22)&(np.mod(yy*78,1)<.22)
        rgb[perforation]*=.70; height[perforation]=-.001; ao[perforation]=.62
        stain=np.clip((grime-.5)*2,0,1)*np.exp(-((yy-.17)/.14)**2)
        rgb *= (1-stain[:,:,None]*np.array((.22,.25,.32)))
    elif key in ('Moss','Leaves'):
        veins=np.sin(xx*80+np.sin(yy*21)*4)*.02
        rgb += veins[:,:,None]; height=(grime-.5)*.003
        smooth[:]=.07; alpha=np.clip((broad-.26)*5,0,1)
    elif key=='Graffiti':
        # Original abstract tags; no real brands, words or copied marks.
        strokes=np.zeros((n,n),dtype=np.float32)
        for k in range(6):
            line=np.exp(-((yy-(.22+.09*k+.11*np.sin(xx*12+k)))/.018)**2)
            strokes=np.maximum(strokes,line*(xx>.09)*(xx<.91))
        alpha=np.clip(strokes*1.4+(fine-.5)*.13,0,1); height=strokes*.0001
        rgb*= (1+(broad-.5)*.27)[:,:,None]
    elif key=='Scorch':
        alpha=np.clip((.65-np.sqrt((xx-.5)**2+(yy-.5)**2))*3.5+(grime-.5)*.55,0,1)
        height*=.4; smooth[:]=.04
    elif key in ('Water','Glass'):
        ripples=np.sin(xx*31+yy*23+grime*2)*.0005
        height=ripples if key=='Water' else height*.1; smooth[:]=.87
        alpha=np.full((n,n),.66 if key=='Water' else .20,dtype=np.float32)
    elif key=='FixtureGlow':
        bands=np.cos(xx*math_tau(4))*.025
        rgb += bands[:,:,None]; smooth[:]=.46; height*=.2
    metres=4 if key in ('Terrazzo','Tile','Concrete','Plaster','Block','FacadeStone','Asphalt','Wood') else 2
    gy,gx=np.gradient(height,n and metres/n)
    normal=np.stack((-gx,-gy,np.ones_like(height)),axis=-1)
    normal/=np.sqrt((normal*normal).sum(axis=-1))[:,:,None]
    mask=np.stack((metallic,ao,np.zeros_like(ao)),axis=-1)
    save(os.path.join(folder,'MALL_'+key+'_BaseColor.png'),'MALL_'+key+'_BaseColor',rgb,alpha)
    save(os.path.join(folder,'MALL_'+key+'_Normal.png'),'MALL_'+key+'_Normal',normal*.5+.5,1,True)
    save(os.path.join(folder,'MALL_'+key+'_Mask.png'),'MALL_'+key+'_Mask',mask,smooth,True)
    print('MERIDIAN_TEXTURE',key,n)

def math_tau(n): return n*np.pi*2

def generate(output, force=False):
    folder=os.path.join(output,'Textures'); os.makedirs(folder,exist_ok=True)
    for key,n in SETS.items():
        if force or not all(os.path.exists(os.path.join(folder,'MALL_'+key+'_'+s+'.png')) for s in ('BaseColor','Normal','Mask')):
            make(key,n,folder)
