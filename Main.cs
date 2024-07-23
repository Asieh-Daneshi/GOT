using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// *****************************************************************************************************************************************************************************
public class Main : MonoBehaviour
{
    public GameObject[] avatarPrefabs;																						  // the original avatars that we copy several times 
	public Material[] clothingMaterialTemplates;
    public int numberOfDuplicates;												     									  // number of times that each avatar must be duplicated
	public int stimSize;	
    public float gridSpacing;																								 // the common distance between each pair of avatars
	private Animator[] Animators;											   							  			   // animations of the avatars that we are going to control
	private GameObject[] myAvatars;

    private Vector3[] myPos;																											   			   // position of one avatar 
    private int[] posList;					                										 // list of positions of all avatars (elements of this list come from myPos)
	private int[] randomList;
	private int[] RightLeft;
    private int avatarCounter = 0;			                                   							       			   // a counter for the avatars that we make in the code
	public int numTrialsTrain = 20;
	public int numTrialsTest = 300;
	public float referenceTime1;
	public float trialDuration=6f;
	
	public Material yellowMaterial;
	public Material blueMaterial;
	
	public float[,] responses;
	
	public GameObject fixationSign;	
    private IEnumerator Start()
    {
		myPos = new Vector3[stimSize * stimSize];
        posList = new int[stimSize * stimSize];
		randomList = new int[stimSize * stimSize];
		// RightLeft = new int[stimSize * stimSize];

        // Step 1: Generate Positions
        yield return StartCoroutine(GeneratePositions());

        // Step 2: Instantiate Avatars
        yield return StartCoroutine(InstantiateAvatars());
    }

    private IEnumerator GeneratePositions()						   			    // in this coroutine, we generate an array of positions that each avatar will be located (myPos)
    {
        int counter = 0;
		// int randRightLeft=0;
		
		for (int i1 = 0; i1 < stimSize; i1++)
        {
            for (int i2 = 0; i2 < stimSize; i2++)
            {
                float randomXOffset = Random.Range(-75f, 75f);
                float randomZOffset = Random.Range(-75f, 75f);
                myPos[counter] = new Vector3(i1 * gridSpacing + randomXOffset-1500, 0f, i2 * gridSpacing + randomZOffset+6000);
				// randRightLeft=Random.Range(0,10);
				// if (randRightLeft < 5)		// left hand
					// RightLeft[counter] = Random.Range(10,15);	// a number between 10 and 14
				// else		// right hand
					// RightLeft[counter] = Random.Range(3,10); 	// a number between 3 and 9
							
						
                counter++;
                yield return null; // Yield each iteration
            }
        }
		
        // for (int i1 = 0; i1 < avatarPrefabs.Length; i1++)
        // {
            // for (int i2 = 0; i2 < numberOfDuplicates; i2++)
            // {
                // float randomXOffset = Random.Range(-75f, 75f);
                // float randomZOffset = Random.Range(-75f, 75f);
                // myPos[counter] = new Vector3(i1 * gridSpacing + randomXOffset, 0f, i2 * gridSpacing + randomZOffset);
                // counter++;
                // yield return null; // Yield each iteration
            // }
        // }
    }

    private IEnumerator InstantiateAvatars()
    {
		Animators = new Animator[stimSize * stimSize];
		myAvatars = new GameObject[stimSize * stimSize];
        for (int i3 = 0; i3 < stimSize * stimSize; i3++)			// in this loop, I make a list of numbers between 0 and expected total number of avatars
        {
            posList[i3] = i3;
			randomList[i3] = i3;
            yield return null; // Yield each iteration
        }

        // int totalAvatars = avatarPrefabs.Length * numberOfDuplicates;
        int posCounter = 0;
		// in this nested loop, first, we shuffle the elements of "posList", so that copies of each avatar be located randomly, not in a queue behid that avatar. Then, we copy 
		// original avatars as new avatars, call their animators, and change the color of their clothes randomly!
        int k1, k2;
		for (int row = 0; row < stimSize; row++)		
		{
            for (int i = 0; i < stimSize; i++)
            {
				if (i<10)
				{
					k1=i;
					k2=0;
				}
				else
				{
					k1=i % (avatarPrefabs.Length);
					k2=Mathf.FloorToInt(i/(avatarPrefabs.Length));
					// print(k1+"  ,  "+k2);
				}
                int posIndex = Random.Range(0, stimSize * stimSize - posCounter);
				int randomIndex = Random.Range(0, stimSize * stimSize - posCounter);
                posCounter++;
                Vector3 position = myPos[posList[posIndex]];
                posList[posIndex] = posList[stimSize * stimSize - posCounter];
				randomList[posIndex] = randomList[stimSize * stimSize - posCounter];
                GameObject newAvatar = Instantiate(avatarPrefabs[k1], position, Quaternion.identity, transform);
                Animator newAnimator = newAvatar.GetComponent<Animator>();
                // print(row * (stimSize) +(k2)*(avatarPrefabs.Length) +k1);
				Animators[row * (stimSize) +(k2)*(avatarPrefabs.Length)+k1]=newAnimator;
				myAvatars[row * (stimSize) +(k2)*(avatarPrefabs.Length)+k1]=newAvatar;
				// print(row * (stimSize) +(k2)*(avatarPrefabs.Length)+k1);
                ApplyRandomColorVariation(row * (stimSize) +(k2)*(avatarPrefabs.Length)+k1,newAvatar);
				Animators[row * (stimSize) +(k2)*(avatarPrefabs.Length)+k1].SetInteger("LR", 0);
                // newAvatar.tag = $"agent{avatarCounter + 1}";
				// ChangeGlovesColor(newAvatar.transform);
                avatarCounter++;
                yield return null; // Yield each iteration
            }
        }
		// Step 3: Deactivate original avatars
        DeactivateOriginalAvatars();
		// Int his loop, we assign a name for each animator, and initiate the animators. "LR" is the parameter that when is not zero starts the animation in the idle mode!
		for (int j = 0; j< (stimSize*stimSize);j++)
		{
			Animators[j].gameObject.name="Animator"+(j+1);
			// Animators[j].SetInteger("LR", RightLeft[j]);
		}
		
		yield return StartCoroutine(ControlAvatars(myAvatars));		                               // "StartCoroutine" is a coroutine that controls the timing of the actions of avatars
    }

	private IEnumerator ControlAvatars(GameObject[] MyAvatar)
    {
		// RightLeft = new int[stimSize * stimSize];
		
		responses = new float[400,10];
		for (int trial=0; trial< numTrialsTrain; trial++)		// Training session
		{
			print("trial: "+ trial);
			RightLeft = new int[stimSize * stimSize];
			int count=0;
			int randRightLeft=0;
			
			for (int i1 = 0; i1 < stimSize; i1++)
			{
				for (int i2 = 0; i2 < stimSize; i2++)
				{
					ApplyBlackPampons(MyAvatar[count]);
					RightLeft[count]=0;
					randRightLeft=Random.Range(0,10);
					if (randRightLeft < 5)		// left hand
						RightLeft[count] = Random.Range(10,15);	// a number between 10 and 14
					else		// right hand
						RightLeft[count] = Random.Range(3,10); 	// a number between 3 and 9
						

					Animators[count].SetInteger("LR", RightLeft[count]);
					count++;
				}
				
			}
			
			
			yield return new WaitForSeconds(2f);	                                                                         // the time before avatars raise their hand
			for (int j = 0; j< (stimSize*stimSize);j++)																		         // avatars raise their hand and keep it up until further notice
			{
				Animators[j].SetInteger("O", 0);
				Animators[j].SetInteger("U", RightLeft[j]);
				Animators[j].SetInteger("KU", RightLeft[j]);
				ApplyColorPampons(MyAvatar[j],RightLeft[j]);
			}
			 print("Raise");
			
			// yield return new WaitForSeconds(3f);							 															// the time that avatars keep their hands up
			
						referenceTime1=Time.time;
				
				
				
				
				
			while(!(Input.GetKey(KeyCode.RightArrow)|Input.GetKey(KeyCode.LeftArrow))& (Time.time-referenceTime1)<trialDuration)
			{
				yield return new WaitForSeconds(Time.deltaTime);
			}
			if(Input.GetKey(KeyCode.RightArrow))
			{
				responses[trial, 5] = 1;
				Input.ResetInputAxes();
			}
			else if(Input.GetKey(KeyCode.LeftArrow))
			{
				responses[trial, 5] = 2;
				Input.ResetInputAxes();
			}
			else
			{
				responses[trial, 5] = 0;
			}
			print("Asi"+trial+" , "+responses[trial,5]);
			
			for (int j = 0; j< (stimSize*stimSize);j++)								   
			{
				Animators[j].gameObject.name="Animator"+(j+1);
				Animators[j].SetInteger("LR", 0);
				Animators[j].SetInteger("U", 0);
				Animators[j].SetInteger("KU", 0);
				Animators[j].SetInteger("D", RightLeft[j]);
				Animators[j].SetInteger("I", RightLeft[j]);
			}
			 print("Down");
			
			yield return new WaitForSeconds(1f); // the time that avatars have already lowered their hands and stay in their idle posture before returning to their original posture
			for (int j = 0; j< (stimSize*stimSize);j++)								   // here, avatars lower their hands and go back to their original posture, waiting for the next trial to start
			{
				Animators[j].gameObject.name="Animator"+(j+1);
				Animators[j].SetInteger("D", 0);
				Animators[j].SetInteger("I", 0);
				Animators[j].SetInteger("O", RightLeft[j]);
			}
			yield return new WaitForSeconds(3f);
			print("Back");
			for (int j = 0; j< (stimSize*stimSize);j++)								   // here, avatars lower their hands and go back to their original posture, waiting for the next trial to start
			{
				Animators[j].gameObject.name="Animator"+(j+1);
				Animators[j].SetInteger("O", 0);
			}
			fixationSign.SetActive(true);		// fixation cross is on for "fixationDuration" seconds
			yield return new WaitForSeconds(1f);
			fixationSign.SetActive(false);
		}
    }

    private void DeactivateOriginalAvatars()
    {
        for (int i = 0; i < avatarPrefabs.Length; i++)
        {
            avatarPrefabs[i].SetActive(false);
        }
    }
	
	
	private void ApplyRandomColorVariation(int q, GameObject newAvatar)
    {
		// ChangeGlovesColor(newAvatar.transform);
		newAvatar=myAvatars[q];
        foreach (Renderer renderer in newAvatar.GetComponentsInChildren<Renderer>())
        {
            if (!renderer.gameObject.name.Contains("Body") && 
                !renderer.gameObject.name.Contains("Hair") && 
                !renderer.gameObject.name.Contains("Eye"))
            {
				// print(renderer.gameObject.name);
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material newMaterial = Instantiate(clothingMaterialTemplates[Random.Range(0, clothingMaterialTemplates.Length)]);

                    Color randomColor = GenerateNonYellowBlue();
                    newMaterial.color = randomColor;
					newMaterials[i] = newMaterial;
                }
                renderer.materials = newMaterials;
			}	
        }
		// ChangeGlovesColor(newAvatar.transform);
    }
	

    private void ApplyColorPampons(GameObject newAvatar1, int myRL)
    {
		// ChangeGlovesColor(newAvatar.transform);
        foreach (Renderer renderer in newAvatar1.GetComponentsInChildren<Renderer>())
        {		
			if ((renderer.gameObject.name.Contains("g_RetopoGroup1_RetopoGroup1") ||
				renderer.gameObject.name.Contains("g_RetopoGroup2_RetopoGroup2") ||
				renderer.gameObject.name.Contains("SphereL")) && myRL>=10 && myRL<=14)
            {
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material newMaterial = Instantiate(clothingMaterialTemplates[Random.Range(0, clothingMaterialTemplates.Length)]);

                    // Color randomColor = GenerateNonYellowBlue();
					// Color randomColor = new Vector4(0f,0.5f,0f);
					newMaterial.color = Color.yellow;
                    newMaterials[i] = newMaterial;
                }
                renderer.materials = newMaterials;
			}
			if ((renderer.gameObject.name.Contains("g_RetopoGroup3_RetopoGroup3") ||
				renderer.gameObject.name.Contains("g_RetopoGroup4_RetopoGroup4") ||
				renderer.gameObject.name.Contains("SphereR")) && myRL>=3 && myRL<=9)
            {
				// print(renderer.gameObject.name);
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material newMaterial = Instantiate(clothingMaterialTemplates[Random.Range(0, clothingMaterialTemplates.Length)]);

                    // Color randomColor = GenerateNonYellowBlue();
					// Color randomColor = new Vector4(0f,0.5f,0f);
					newMaterial.color = Color.blue;
                    newMaterials[i] = newMaterial;
                }
                renderer.materials = newMaterials;
			}
        }
		// ChangeGlovesColor(newAvatar.transform);
    }
	
	
	private void ApplyBlackPampons(GameObject newAvatar1)
    {
		// ChangeGlovesColor(newAvatar.transform);
        foreach (Renderer renderer in newAvatar1.GetComponentsInChildren<Renderer>())
        {
			renderer.enabled = true;
			if (renderer.gameObject.name.Contains("g_RetopoGroup1_RetopoGroup1") ||
				renderer.gameObject.name.Contains("g_RetopoGroup2_RetopoGroup2") ||
				renderer.gameObject.name.Contains("SphereL"))
            {
				// print(renderer.gameObject.name);
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material newMaterial = Instantiate(clothingMaterialTemplates[Random.Range(0, clothingMaterialTemplates.Length)]);

                    // Color randomColor = GenerateNonYellowBlue();
					// Color randomColor = new Vector4(0f,0.5f,0f);
					newMaterial.color = Color.black;
					// Color color = newMaterial.color;
					// color.a = 0f;
					// newMaterial.color = color;
                    newMaterials[i] = newMaterial;
                }
                renderer.materials = newMaterials;

			}
			if (renderer.gameObject.name.Contains("g_RetopoGroup3_RetopoGroup3") ||
				renderer.gameObject.name.Contains("g_RetopoGroup4_RetopoGroup4") ||
				renderer.gameObject.name.Contains("SphereR"))
            {
				// print(renderer.gameObject.name);
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material newMaterial = Instantiate(clothingMaterialTemplates[Random.Range(0, clothingMaterialTemplates.Length)]);

                    // Color randomColor = GenerateNonYellowBlue();
					// Color randomColor = new Vector4(0f,0.5f,0f);
					newMaterial.color = Color.black;
					// newMaterial.color = Color.clear;
					// newMaterial.color = new Color(1f, 0f, 0f, -0.001f);
					// Color color = newMaterial.color;
					// color.a = 0f;
					// newMaterial.color = color;
                    newMaterials[i] = newMaterial;
                }
                renderer.materials = newMaterials;

			}
        }
		// ChangeGlovesColor(newAvatar.transform);
    }

    private Color GenerateNonYellowBlue()
    {
		// hue: 0-60:red; 61-120:yellow; 121-180:green; 181-240:cyan; 241-300: blue; 301-360:magenta
        float hue, sat, val;
		hue=2;
		while((hue>0.08f && hue<0.18f) || (hue>0.2f && hue<0.5f) || (hue>0.64f) )
		{ 
			hue = Random.Range(0f, 1f);
		}
        sat = Random.Range(0.2f, 1f);		// how strong is the color
        val = Random.Range(0.3f, 0.7f);		// how bright is the color
        
        return Color.HSVToRGB(hue, sat, val);
    }
}