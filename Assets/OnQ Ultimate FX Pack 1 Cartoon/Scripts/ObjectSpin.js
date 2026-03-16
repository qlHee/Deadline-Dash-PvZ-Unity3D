var DegreesX : float = 0.0f;
var DegreesY : float = 0.0f;
var DegreesZ : float = 0.0f;

function Start(){
	transform.localRotation = Quaternion.identity;
}

function Update() {
	
    transform.Rotate(DegreesX * Time.deltaTime, DegreesY * Time.deltaTime, DegreesZ * Time.deltaTime);

}