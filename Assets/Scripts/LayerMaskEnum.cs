using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LayerMaskName { FLAT, CHARACTER };

public static class LayerMasks {


    private static Dictionary<LayerMaskName, LayerMask> layerMaskDic = null;

    private static Dictionary<LayerMaskName, LayerMask> GetDictionary() {

        if (layerMaskDic == null) { 
            layerMaskDic = new Dictionary<LayerMaskName, LayerMask>();
            layerMaskDic.Add(LayerMaskName.FLAT, LayerMask.GetMask("Flat"));
            layerMaskDic.Add(LayerMaskName.CHARACTER, LayerMask.GetMask("Character"));
        }

        return layerMaskDic;

    }

    public static LayerMask GetLayerMask(LayerMaskName _maskName) { 
    
        return GetDictionary()[_maskName];

    }
    

}
