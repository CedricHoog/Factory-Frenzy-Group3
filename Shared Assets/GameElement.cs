abstract class GameElement
{
    //Possible parameters : position, size, orientation, endingPosition, movementTime
    //force could also be added for PlatformTrampoline, Bumper. Thought it doesn't seems required.

    private int id; //should be unique accross all game elements loaded. 
    private string name; //used in tooltip and for some checks
    private string description; //used in the tooltip. Is string adapted ???
    private Vector3 position;
    private Vector3 size; //might not be necessary or possible depending on the assets
    private Vector3 orientation; //this could be reduced to only Y rotation for simplicity.


    //some function implementation could be done here
    public setId(int id);
    public int getId();

    public setName(string name); // TODO ? is this necessary ? This should used be called when component is initialized
    public string getName();
    
    public setDescription(string description); // TODO ? is this necessary ? This should used be called when component is initialized
    public string getDescription();

    public setPosition(Vector3 position);
    public Vector3 getPosition();

    public setSize(Vector3 size);
    public Vector3 getSize();

    public setOrientation(Vector3 orientation);
    public Vector3 getOrientation();

    //a save function might also be necessary

}
/* All 9 Game Elements */
class Platform : GameElement
{
}

class PlatformMoving : GameElement
{
    //this element has extra parameters
    private int movementTime;
    private Vector3 endingPosition;

    public setMovementTime(int movementTime);
    public int getMovementTime();

    public setEndingPosition(Vector3 endingPosition);
    public Vector3 getEndingPosition();
}

class PlatformTrampoline : GameElement
{
    //we are not required to change the force so could go unused
    private int force;
    public setForce(int Force);
    public int getForce();
}

class TrapBumper : GameElement
{
    //we are not required to change the force so could go unused
    private int force;
    public setForce(int Force);
    public int getForce();
}

class TrapFan : GameElement
{
    //we are not required to change the force & activation zone so could go unused
    private int force;
    private Vector3 windZone;
    public setForce(int Force);
    public int getForce();
    public setWindZone(Vector3 windZone);
    public Vector3 getWindZone();
}

class TrapThrower : GameElement
{
    //we are not required to change the range so could go unused
    private int range;
    public setRange(int range);
    public int getRange();
}

class Checkpoint : GameElement
{
}

class PlatformStart : GameElement
{
}

class PlatformEnd : GameElement
{
}