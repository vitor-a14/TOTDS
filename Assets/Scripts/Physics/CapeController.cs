using UnityEngine;

public class CapeController : PhysicsObject
{
    public Cloth cloth;
    public Material material;
    //If you want to use start method, don't forget to call InitializePhysics

    private void LateUpdate()
    {
        UpdatePhysics();
        Physics.clothGravity = -mainForceDirection * 12;
    }

    public void ChangeWorldPos(Vector3 newOffset, Vector3 translation) {
        cloth.ClearTransformMotion();
    }
}
