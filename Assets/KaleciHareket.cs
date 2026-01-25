using UnityEngine;

public class KaleciHareket : MonoBehaviour
{
    // Kaleci ne kadar hızlı gidip gelsin?
    public float hiz = 2.0f;
    
    // Kaleci merkezden ne kadar uzağa açılsın? (Genişlik)
    public float mesafe = 3.0f;

    // Başlangıç pozisyonunu hafızada tutalım
    Vector3 baslangicPozisyonu;

    void Start()
    {
        // Oyun başladığı an, kaleciyi koyduğun yeri "Merkez" kabul et.
        baslangicPozisyonu = transform.position;
    }

    void Update()
    {
        // MATEMATİK DERSİ:
        // Mathf.Sin(Time.time) -> Zaman ilerledikçe -1 ile 1 arasında sayı üretir.
        // Bunu 'mesafe' ile çarparsan, örneğin -3 ile 3 arasında gidip gelir.
        float yeniX = baslangicPozisyonu.x + Mathf.Sin(Time.time * hiz) * mesafe;

        // Hesapladığımız yeni X pozisyonunu kaleciye uygula.
        // Y ve Z (Yükseklik ve Derinlik) değişmesin, sabit kalsın.
        transform.position = new Vector3(yeniX, baslangicPozisyonu.y, baslangicPozisyonu.z);
    }
}