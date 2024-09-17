using System.Collections.Generic;

public static class CharaAniTrigger
{
    private static Dictionary<Character.STATE, string> aniTriggerDic = null;

    public static Dictionary<Character.STATE, string> GetCharAniTriggerDic() {

        if (aniTriggerDic == null) initDictionary();

        return aniTriggerDic;
    
    }

    private static void initDictionary() { 
    
        aniTriggerDic = new Dictionary<Character.STATE, string>();

        //모든 상태변수를 지정.
        aniTriggerDic.Add(Character.STATE.IDLE, "isIdle");
        aniTriggerDic.Add(Character.STATE.ACTING, "isWalking");
        aniTriggerDic.Add(Character.STATE.DRAG, "isTaking");
        aniTriggerDic.Add(Character.STATE.TOUCHED, "isTouched");

    }


}
