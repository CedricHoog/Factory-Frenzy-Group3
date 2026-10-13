abstract class GameElement
{
    //Possible parameters : position, size, orientation, endingPosition, movementTime
    //force could also be added for PlatformTrampoline, Bumper. Thought it doesn't seems required.

    private int Id;
    private string name;
    private Vector3 position;
    private Vector3 size;
    private Vector3 orientation;


    //some function implementation could be done here
    public setId(int id);
    public int getId();

    public setName(string name); // TODO ? is this necessary ?
    public string getName();

    public setPosition(Vector3 position);
    public Vector3 getPosition();

    public setSize(Vector3 size);
    public Vector3 getSize();

    public setOrientation(Vector3 orientation);
    public Vector3 getOrientation();

    //a save function might also be necessary

}

class Platform : GameElement
{
    //le nom est propre à l'element. (utile pour des checks avec l'ui pour voir ce qu'il faut changer + pour la sauvegarde)
}

class PlatformMoving : GameElement
{
    private int movementTime;
    private Vector3 endingPosition;

    public setMovementTime(int movementTime);
    public int getMovementTime();

    public setEndingPosition(Vector3 endingPosition);
    public Vector3 getEndingPosition();
}