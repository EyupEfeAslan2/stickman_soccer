using UnityEngine;

public class TopKontrol : MonoBehaviour
{
    // Topun fizik motoruna ulaşmak için bir değişken
    Rigidbody rb; 
    
    // Şutun ne kadar sert olacağı (Editörden değiştirebilirsin)
    public float vurusGucu = 5000f; 
    
    // Topun ne kadar havaya kalkacağı
    public float havalanmaGucu = 2000f;

    void Start()
    {
        // Oyun başlayınca topun üzerindeki Rigidbody bileşenini bul ve hafızaya al
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Eğer klavyeden "Boşluk" (Space) tuşuna basılırsa
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SutCek();
        }
    }

    void SutCek()
    {
        // Topa kuvvet uygula: İleriye (Forward) ve Yukarıya (Up)
        // AddForce komutu fizik motoruna "bunu it" der.
        rb.AddForce(Vector3.forward * vurusGucu + Vector3.up * havalanmaGucu);
    }
}