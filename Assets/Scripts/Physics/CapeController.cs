using UnityEngine;

public class CapeController : PhysicsObject
{
    public Cloth cloth;
    //If you want to use start method, don't forget to call InitializePhysics

    void LateUpdate()
    {
        cloth.externalAcceleration = -mainForceDirection * 6;
    }
}
