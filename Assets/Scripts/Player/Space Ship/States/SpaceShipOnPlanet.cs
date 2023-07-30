using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipOnPlanet : SpaceShipState
{
    public SpaceShipOnPlanet(SpaceShipController spaceShip, SpaceShipStateMachine stateMachine) : base(spaceShip, stateMachine) { }

    public override void Enter() { }

    public override void Exit() { }

    public override void Interact() {
        spaceShip.StateMachine.ChangeState(spaceShip.IdleState);
    }

    public override void StateUpdate() { } 

    public override void StateFixedUpdate() { } 

}
