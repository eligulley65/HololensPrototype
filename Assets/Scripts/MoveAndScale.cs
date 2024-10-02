using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAndScale
{
    GameObject insideCut;
    GameObject outsideCut;
    GameObject finalObject;
    
    public MoveAndScale(GameObject inside, GameObject outside, GameObject final){
        insideCut = inside;
        outsideCut = outside;
        finalObject = final;
    } 
    
    public void MoveObjects(){
        //make the two objects invisible
       //insideCut.GetComponent<MeshRenderer>().enabled = false; 
       //outsideCut.GetComponent<MeshRenderer>().enabled = false; 

       //move the objects over
       //insideCut.transform.position = finalObject.transform.position;
       //outsideCut.transform.position = finalObject.transform.position;
       //insideCut.transform.rotation = finalObject.transform.rotation;
       //outsideCut.transform.rotation = finalObject.transform.rotation;
       //insideCut.transform.localScale = finalObject.transform.localScale;
       //outsideCut.transform.localScale = finalObject.transform.localScale;
    }
}
