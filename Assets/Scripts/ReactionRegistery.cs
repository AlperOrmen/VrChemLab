using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReactionRegistry : MonoBehaviour {
    public static ReactionRegistry Instance;
    public List<LiquidRecipe> allRecipes; 

    private void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

    public LiquidRecipe CheckReaction(LiquidData data1, LiquidData data2) {
        if (data1 == null || data2 == null) return null;
        return allRecipes.FirstOrDefault(r => 
            r != null && r.inputA != null && r.inputB != null && 
            (
                (r.inputA.name == data1.name && r.inputB.name == data2.name) ||
                (r.inputA.name == data2.name && r.inputB.name == data1.name)
            )
        );
    }
}