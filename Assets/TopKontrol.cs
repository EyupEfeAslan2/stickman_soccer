using UnityEngine;
using UnityEngine.SceneManagement;

public class TopKontrol : MonoBehaviour
{
    Rigidbody rb;
    LineRenderer lr; // Çizgi çizici bileşeni

    [Header("Şut Ayarları")]
    public float gucCarpani = 15f; 
    public float yukseklikCarpani = 3f; 
    public float maksimumGuc = 3000f; 

    // OYUN DURUMU
    bool oyunBitti = false;
    bool nisanAliyor = false; // Mouse basılı mı?

    Vector3 baslangicMousePozisyonu;
    Vector3 bitisMousePozisyonu;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lr = GetComponent<LineRenderer>(); // Line Renderer'ı bul
    }

    void Update()
    {
        // 1. OYUN BİTMİŞ OLSA BİLE RESET ÇALIŞSIN (Fix)
        // Oyun bittiyse otomatiğe bağlarız ama oyuncu manuel R yapmak isterse diye:
        if (Input.GetKeyDown(KeyCode.R))
        {
            MaciTekrarla();
        }

        // Eğer oyun bittiyse veya top hareketliyse şut çektirme
        if (oyunBitti || rb.linearVelocity.magnitude > 0.5f) return;

        // --- ŞUT MEKANİĞİ ---

        // Mouse'a tıklandığı an
        if (Input.GetMouseButtonDown(0))
        {
            nisanAliyor = true;
            baslangicMousePozisyonu = Input.mousePosition;
            lr.enabled = true; // Çizgiyi görünür yap
            lr.positionCount = 2; // Çizgi 2 noktadan oluşacak (Başlangıç - Bitiş)
        }

        // Mouse basılı tutulurken (Çizgiyi güncelle)
        if (Input.GetMouseButton(0) && nisanAliyor)
        {
            Vector3 suankiMouse = Input.mousePosition;
            Vector3 fark = baslangicMousePozisyonu - suankiMouse;
            
            // Çizginin Başlangıcı: Topun olduğu yer
            lr.SetPosition(0, transform.position);

            // Çizginin Bitişi: Topun gideceği tahmini yön (Fark vektörünü ekliyoruz)
            // Y eksenini 0 yapıyoruz ki çizgi havaya kalkmasın, yerde görünsün
            Vector3 hedefGosterge = transform.position + new Vector3(fark.x, 0, fark.y) * 0.05f; 
            lr.SetPosition(1, hedefGosterge);
        }

        // Mouse bırakıldığı an (Şut!)
        if (Input.GetMouseButtonUp(0) && nisanAliyor)
        {
            nisanAliyor = false;
            lr.enabled = false; // Çizgiyi kapat
            bitisMousePozisyonu = Input.mousePosition;
            SutCek();
        }
    }

    void SutCek()
    {
        Vector3 fark = baslangicMousePozisyonu - bitisMousePozisyonu;
        
        // Çok küçük dokunuşlarda şut çekmesin (Yanlış tıklamalar için)
        if (fark.magnitude < 10f) return;

        Vector3 vurusYonu = new Vector3(fark.x, 0, fark.y);
        
        // Aşırtma etkisi
        vurusYonu.y = fark.magnitude / yukseklikCarpani;

        Vector3 sonKuvvet = vurusYonu * gucCarpani;

        // Güç limiti (Clamp)
        if (sonKuvvet.magnitude > maksimumGuc)
        {
            sonKuvvet = sonKuvvet.normalized * maksimumGuc;
        }

        rb.AddForce(sonKuvvet);
    }

    void OnTriggerEnter(Collider other)
    {
        if (oyunBitti) return;

        if (other.CompareTag("Kaleici"))
        {
            Debug.Log("GOOOOOLLL!!! ⚽");
            OyunBitir();
            // Golden sonra sevinmek için 3 saniye bekle
            Invoke("MaciTekrarla", 3.0f);
        }
        
        // Out Kontrolü
        if (other.CompareTag("Disari"))
        {
            Debug.Log("Out! ❌");
            OyunBitir();
            // Out olunca 1.5 saniye sonra resetle (Otomatik Reset Fix)
            Invoke("MaciTekrarla", 1.5f);
        }
    }

    void OyunBitir()
    {
        oyunBitti = true;
        lr.enabled = false; // Ok açıksa kapat
        // Topu durdur
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void MaciTekrarla()
    {
        // Invoke ile çağrılan resetlemeleri iptal et (Üst üste binmesin)
        CancelInvoke(); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}