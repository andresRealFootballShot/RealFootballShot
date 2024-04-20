using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseModel : MonoBehaviour
{
    public static ModelName.Name choosedModel;
    public static void ChooseRandomModel()
    {
        choosedModel = MyFunctions.RandomEnum<ModelName.Name>();

    }
    public static void Choose(ModelName.Name modelName)
    {
        choosedModel = modelName;

    }
}
