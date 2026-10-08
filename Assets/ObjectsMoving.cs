using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;


public class ObjectsMoving : MonoBehaviour
{

public GameObject player;

public CardSystem cardSystem;

        public Camera FPCamera;

        public float maxDistance = 3f;
        public Transform holdPoint; // Prazan GameObject ispred kamere

        private Transform heldObject;

        public float throwingForce;

        public void pushingObject(Rigidbody obj)
        {
            obj.isKinematic = false;
            heldObject.SetParent(null);
            heldObject = null;

            obj.AddForce(transform.forward * throwingForce, ForceMode.Impulse);
        }

        public GameObject eatBarGO;
        public Image eatingBar;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (heldObject == null)
{
    bool canReplaceCard = false;
    CardPickup lookedPickup = null;

    Ray ray = FPCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
    {
        // ---- FOOD (tvoj postojeći kod, nepromenjen) ----
        if (hit.collider.CompareTag("food") && Input.GetMouseButtonDown(0))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                heldObject = hit.transform;
                rb.isKinematic = true;
                heldObject.SetParent(holdPoint);
                heldObject.position = Vector3.MoveTowards(heldObject.position, holdPoint.position, 2 * Time.deltaTime);
            }
        }

        // ---- CARD ----
        if (hit.collider.CompareTag("CARD"))
        {
            CardPickup pickup = hit.collider.GetComponent<CardPickup>();

            if (pickup != null)
            {
                    lookedPickup = pickup;

                if (cardSystem.IsFull)
                {
                    canReplaceCard = true; // pali indikator na selektovanoj kartici

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        cardSystem.ReplaceSelectedCard(pickup.cardSprite, pickup.word);
                        Destroy(hit.collider.gameObject);
                        canReplaceCard = false;
                        lookedPickup = null;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    if (cardSystem.AddCard(pickup.cardSprite, pickup.word))
                    {
                        Destroy(hit.collider.gameObject);
                        lookedPickup = null;
                    }
                }
            }
        }
    }

    cardSystem.ShowReplaceIndicator(canReplaceCard); // svaki frame: upali ako gledaš prop, inače ugasi
    
    if (lookedPickup != null)
    cardSystem.ShowPickupInfo(lookedPickup.cardSprite, lookedPickup.word);
else
    cardSystem.HidePickupInfo();
}




            // Ako već držimo, prati poziciju
            else
            {

                cardSystem.ShowReplaceIndicator(false);
                cardSystem.HidePickupInfo();


                Rigidbody rb = heldObject.GetComponent<Rigidbody>();


                if (Input.GetMouseButtonUp(0))
                {


                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        heldObject.SetParent(null); // pusti objekat
                        heldObject = null;
                    }
                }
                else
                {
                    // Pomeraj ga ka holdPoint ako nije odmah tamo
                    heldObject.position = Vector3.Lerp(heldObject.position, holdPoint.position, Time.deltaTime * 10f);
                }

                // JEDENJE HRANE KOJA SE DRZI U RUCI
                if (Input.GetKey(KeyCode.E))
                {

                    eatBarGO.SetActive(true);


                    eatingBar.fillAmount += 0.2f * Time.deltaTime;




                    if (eatingBar.fillAmount >= 1f)
                    {

                        
                        //player.GetComponent<Health>().changeHealth(heldObject.GetComponent<FoodData>().damage);

                            
                        
                        
                        eatingBar.fillAmount = 0;

                    }
                }
                else // Ako pusti hranu - gasi bar i resetuj progress
                {

                    eatBarGO.SetActive(false);

                    eatingBar.fillAmount = 0;



                }

                if (Input.GetMouseButton(1))
                {
                    pushingObject(rb);
                }
            }
            
    }
}
