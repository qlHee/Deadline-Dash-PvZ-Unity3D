var FXLoop : GameObject;
var intervalMin : float = 0;
var intervalMax : float = 0;
var positionFactor : float = 0;
var killtime : float = 0;

function Start() {

	InvokeRepeating("Burst",0.0,Random.Range(intervalMin, intervalMax));

}

function Burst () {
	if(gameObject.active==false){return;}
	var clone : GameObject;
	var pos = transform.position;
		pos.x += Random.Range(-positionFactor, positionFactor);
		pos.y += Random.Range(-positionFactor, positionFactor);
		pos.z += Random.Range(-positionFactor, positionFactor);
		
	clone = Instantiate (FXLoop, pos, transform.rotation);
	
	Destroy (clone.gameObject, killtime);


}