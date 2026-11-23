using UnityEngine;

// Hides "field is never used" warnings until you complete the TODOs.
#pragma warning disable 0169, 0414

// CET2000 Lab 05 - Expert E3: the same FSM written as one class per state (the State pattern).
// Idle and Patrol are written for you as examples. YOUR JOB (E3): complete Chase, Attack and Flee,
// then tick "Use State Classes" on the guard to run this version instead of the switch.

public interface IGuardState
{
    GuardState Id { get; }
    void Enter(GuardFSM_Template g);
    void Tick(GuardFSM_Template g, float dt);
    void Exit(GuardFSM_Template g);
}

// One object per state for each guard.
public class GuardStateSet
{
    public readonly IGuardState Idle = new IdleState();
    public readonly IGuardState Patrol = new PatrolState();
    public readonly IGuardState Chase = new ChaseState();
    public readonly IGuardState Attack = new AttackState();
    public readonly IGuardState Flee = new FleeState();
}

public class IdleState : IGuardState
{
    public GuardState Id => GuardState.Idle;
    public void Enter(GuardFSM_Template g) { g.SetColour(new Color(0.65f, 0.65f, 0.65f)); g.StopMoving(); }
    public void Tick(GuardFSM_Template g, float dt)
    {
        g.StopMoving();
        g.Regenerate(dt);
        if (g.CanSee) g.SetState(g.S.Chase);
        else if (g.TimeInState >= g.idleTime) g.SetState(g.S.Patrol);
    }
    public void Exit(GuardFSM_Template g) { }
}

public class PatrolState : IGuardState
{
    public GuardState Id => GuardState.Patrol;
    public void Enter(GuardFSM_Template g) => g.SetColour(new Color(0.25f, 0.55f, 0.95f));
    public void Tick(GuardFSM_Template g, float dt)
    {
        g.PatrolWaypoints(g.patrolSpeed);
        if (g.CanSee) g.SetState(g.S.Chase);
    }
    public void Exit(GuardFSM_Template g) { }
}

public class ChaseState : IGuardState
{
    public GuardState Id => GuardState.Chase;
    public void Enter(GuardFSM_Template g) { g.SetColour(new Color(1f, 0.6f, 0.1f)); g.Shout("Hey!"); }
    public void Tick(GuardFSM_Template g, float dt)
    {
        // TODO E3: same rules as TODO 3, written with g. in front, e.g.
        //   g.MoveTowards(g.PlayerPosition, g.chaseSpeed);
        //   if (g.health < g.fleeHealth) g.SetState(g.S.Flee); else if ...
    }
    public void Exit(GuardFSM_Template g) { }
}

public class AttackState : IGuardState
{
    float cooldown;
    public GuardState Id => GuardState.Attack;
    public void Enter(GuardFSM_Template g) { g.SetColour(new Color(0.9f, 0.2f, 0.2f)); cooldown = 0.2f; }
    public void Tick(GuardFSM_Template g, float dt)
    {
        // TODO E3: same rules as TODO 4. Use the field "cooldown" instead of attackTimer.
    }
    public void Exit(GuardFSM_Template g) => g.StopMoving();
}

public class FleeState : IGuardState
{
    float safe;
    public GuardState Id => GuardState.Flee;
    public void Enter(GuardFSM_Template g) { g.SetColour(new Color(0.6f, 0.35f, 0.85f)); safe = 0f; g.Shout("Retreat!"); }
    public void Tick(GuardFSM_Template g, float dt)
    {
        // TODO E3: same rules as TODO 5. Use the field "safe" instead of safeTimer.
    }
    public void Exit(GuardFSM_Template g) { }
}
