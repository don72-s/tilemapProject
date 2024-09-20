

public abstract class BaseState{

    protected Character.STATE myState { get; private set; }

    public BaseState(Character.STATE _myState) {

        myState = _myState;

    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

}
