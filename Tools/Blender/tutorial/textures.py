"""Original, seamless house PBR maps. Run with Blender's bundled numpy; no downloads."""
import os, struct, zlib
import numpy as np

def png(path, pixels):
    a = np.uint8(np.clip(pixels, 0, 1) * 255)
    h, w, channels = a.shape
    def chunk(tag, data):
        return struct.pack('>I', len(data)) + tag + data + struct.pack('>I', zlib.crc32(tag + data) & 0xffffffff)
    raw = b''.join(b'\0' + row.tobytes() for row in a)
    with open(path, 'wb') as f:
        f.write(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6 if channels == 4 else 2, 0, 0, 0)) + chunk(b'IDAT', zlib.compress(raw, 6)) + chunk(b'IEND', b''))

def generate(folder):
    os.makedirs(folder, exist_ok=True)
    n = 1024
    y, x = np.mgrid[0:n, 0:n] / n
    rng = np.random.default_rng(140021)
    noise = rng.random((n, n)) - .5
    broad = (np.sin(x * 2*np.pi*3 + np.sin(y*2*np.pi*2)) + np.cos(y*2*np.pi*5 + np.cos(x*2*np.pi*2))) * .5
    for key in ['Wood','FloorWood','Fabric','Carpet','Rug','Tile','Lino','Laminate','Plaster','Block','Siding','Shingles','Cardboard','Bark','Paint','Floral']:
        h = .5 + noise*.08
        c = .88 + broad*.025 + noise*.04
        smooth = np.full((n,n), .18)
        if key in ['Wood','FloorWood','Bark']:
            grain = np.sin(2*np.pi*(x*48 + .8*np.sin(y*2*np.pi) + .35*np.sin(y*6*np.pi+x*4*np.pi)))
            fine = np.sin(2*np.pi*(x*173 + .4*np.sin(y*4*np.pi)))
            h = .5 + grain*.045 + fine*.015 + noise*.02
            c = .80 + grain*.065 + fine*.025 + broad*.04
            smooth[:] = .32
            if key == 'FloorWood':
                row = np.floor(x*8)
                seam = ((x*8)%1 < .017) | ((y*3 + (row%3)/3)%1 < .008)
                h -= seam*.12
                c *= np.where(seam, .44, .94 + .06*np.sin(row*12.7))
            if key == 'Bark':
                h = .5 + grain*.14 + fine*.08
                c = .7 + grain*.15 + broad*.09
                smooth[:] = .04
        elif key in ['Fabric','Carpet','Floral','Rug']:
            weave = np.sin(x*2*np.pi*256)*np.sin(y*2*np.pi*256)
            h = .5 + weave*.04 + noise*.06
            c = .9 + weave*.035 + noise*.07 + broad*.025
            smooth[:] = .035
            if key == 'Carpet':
                h += np.sin(x*2*np.pi*128 + np.sin(y*2*np.pi*128))*.035
            if key == 'Floral':
                petal = np.cos(x*2*np.pi*8)*np.cos(y*2*np.pi*8)
                c *= np.where(petal > .65, .7, 1)
            if key == 'Rug':
                diamond = np.abs((x*8)%1-.5)+np.abs((y*8)%1-.5)
                c *= np.where((diamond>.40)&(diamond<.49), .53, 1)
        elif key in ['Tile','Block','Lino','Shingles','Siding']:
            nx, ny = {'Tile':(4,4),'Block':(4,8),'Lino':(4,4),'Shingles':(8,12),'Siding':(1,8)}[key]
            row = np.floor(y*ny)
            u = (x*nx + (row%2)*.5)%1
            v = (y*ny)%1
            seam = (u < .018) | (v < .025)
            if key == 'Siding': seam = v < .035
            h = .5 + noise*.018 - seam*.16
            c = (.9 + noise*.025 + broad*.02)*np.where(seam, .58, 1)
            smooth[:] = .5 if key in ['Tile','Lino'] else .1
            smooth[seam] = .06
            if key == 'Lino':
                alternate = (np.floor(x*nx)+row)%2
                c *= .76 + alternate*.24
                h += noise*.02
            if key in ['Block','Shingles']:
                h += noise*.12
                c += noise*.12
                c *= .92 + .08*np.sin((np.floor(x*nx+(row%2)*.5)+row*7.3)*13)
            if key == 'Siding': h += v*.09
        elif key == 'Laminate':
            fleck = rng.random((n,n)) > .96
            c = .92 + noise*.065 - fleck*.14
            h = .5 + noise*.012
            smooth[:] = .48
        elif key == 'Plaster':
            h = .5 + noise*.10 + broad*.025
            c = .94 + noise*.035 + broad*.025
            smooth[:] = .04
        elif key == 'Cardboard':
            c = .88 + noise*.08 + np.sin(x*2*np.pi*180)*.018
            h = .5 + noise*.04
            smooth[:] = .04
        else:
            h = .5 + noise*.012
            c = .95 + broad*.025 + noise*.015
            smooth[:] = .34
        colour = np.repeat(c[:,:,None],3,axis=2)
        # Tangent-space normal gradients wrap across the seam. Keep relief restrained at real-world scale.
        dx = (np.roll(h,-1,1)-np.roll(h,1,1))*1.5
        dy = (np.roll(h,-1,0)-np.roll(h,1,0))*1.5
        normal = np.stack((-dx,-dy,np.ones_like(h)),axis=2)
        normal /= np.linalg.norm(normal,axis=2)[:,:,None]
        mask = np.stack((np.zeros_like(h),np.clip(.94+(h-.5)*.3,.72,1),np.zeros_like(h),smooth),axis=2)
        png(os.path.join(folder,key+'_BaseColor.png'),colour)
        png(os.path.join(folder,key+'_Normal.png'),normal*.5+.5)
        png(os.path.join(folder,key+'_Mask.png'),mask)
        print('HOUSE_TEXTURE',key)
