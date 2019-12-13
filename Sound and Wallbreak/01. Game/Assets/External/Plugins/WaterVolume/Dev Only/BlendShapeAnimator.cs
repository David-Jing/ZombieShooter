using UnityEngine;
using System.Collections;

public class BlendShapeAnimator : MonoBehaviour
{

    int blendShapeCount;
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    public float blendSpeed = 10f;
  
    int blendIndex = 0;
    float currentAmount = 0;

    private float originalZScale;

    private int blendShapeInfluence = 100;

    void Awake()
    {
        originalZScale = transform.localScale.z;
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    void Start()
    {
        blendShapeCount = skinnedMesh.blendShapeCount;
        //skinnedMeshRenderer.SetBlendShapeWeight(0, 100);
    }

    public void SetWaveScale(float v)
    {
      transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, originalZScale + v);
    }

    public void SetWaveSpeed(float v)
    {
        blendSpeed = v * 100;
    }


    void Update()
    {
        if (blendIndex >= blendShapeCount)
        {
            blendIndex = 0;
        }
        currentAmount = skinnedMeshRenderer.GetBlendShapeWeight(blendIndex);

        if (currentAmount < blendShapeInfluence)
        {
            currentAmount += blendSpeed * Time.deltaTime;
            skinnedMeshRenderer.SetBlendShapeWeight(blendIndex, currentAmount);
            if (blendIndex > 0)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendIndex - 1, blendShapeInfluence - currentAmount);
            }
            else
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendShapeCount - 1, blendShapeInfluence - currentAmount);
            }

        }
        else
        {
            currentAmount = 0;
            blendIndex++;
        }

      
     
    }
}