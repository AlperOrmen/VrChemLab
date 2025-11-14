using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LiquidController : MonoBehaviour
{
    [Header("Temel Bileşenler")]
    [Tooltip("İçerideki sıvıyı gösteren 3D mesh'in MeshRenderer'ı.")]
    public Renderer liquidMeshRenderer;
    [Tooltip("Dökülme noktasını temsil eden boş obje.")]
    public Transform pourOriginTransform;
    [Tooltip("Sıvı akışını çizdirecek Line Renderer.")]
    public LineRenderer liquidStreamLine;

    [Header("Hacim Ayarları")]
    public float maxVolume = 100f;
    [Range(0f, 100f)]
    public float currentVolume = 100f;

    [Header("Dökme Mekaniği")]
    [Tooltip("Dökmeye başlamak için gereken minimum eğim açısı (derece).")]
    public float pourAngleThreshold = 70f;
    [Tooltip("Saniyede ne kadar sıvının döküleceği (hacim/sn).")]
    public float pourRate = 20f;

    [Header("Sıvı Akışı (Line Renderer) Ayarları")]
    [Tooltip("Sıvının dökülme başlangıç hızı.")]
    public float streamVelocity = 1.0f;
    [Tooltip("Çizginin pürüzsüzlüğü (kaç noktadan oluşacağı).")]
    public int streamResolution = 20;
    [Tooltip("Sıvının çarpabileceği zemin, masa, diğer bardaklar vb. katmanlar.")]
    public LayerMask pourTargetLayer;

    [Header("Shader Ayarları")]
    [Tooltip("Shader'daki doluluk seviyesini kontrol eden değişkenin adı (Örn: _Fill)")]
    public string fillShaderProperty = "_Fill";

    // Performans için MaterialPropertyBlock
    private MaterialPropertyBlock propBlock;
    private int fillPropertyID;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        propBlock = new MaterialPropertyBlock();
        fillPropertyID = Shader.PropertyToID(fillShaderProperty);

        if (liquidMeshRenderer == null)
        {
            Debug.LogError("Liquid Mesh Renderer atanmamış!", this);
            this.enabled = false;
            return;
        }

        // Line Renderer ve Pour Origin SADECE döken bardak için gereklidir
        if (pourOriginTransform == null || liquidStreamLine == null)
        {
            Debug.LogWarning("Bu kap bir alıcı olarak ayarlandı (Dökme bileşenleri eksik).", this);
        }
        else
        {
            liquidStreamLine.enabled = false;
        }

        UpdateShaderFill();
    }

    void Update()
    {
        // Sadece dökme bileşenleri tam olanlar bu kontrolü yapsın
        if (pourOriginTransform == null || liquidStreamLine == null) return;
        
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle > pourAngleThreshold && currentVolume > 0)
        {
            HandlePouring();
        }
        else
        {
            StopPouring();
        }
    }

    private void HandlePouring()
    {
        // 1. Bu kare ne kadar sıvı döküleceğini hesapla
        float amountToPour = pourRate * Time.deltaTime;

        // 2. Hacmi azalt
        currentVolume -= amountToPour;
        currentVolume = Mathf.Max(currentVolume, 0f);

        // 3. Shader'daki doluluğu güncelle
        UpdateShaderFill();

        // 4. Line Renderer'ı göster ve güncelle (ve ne kadar döktüğümüzü bildir)
        liquidStreamLine.enabled = true;
        UpdateLineRendererStream(amountToPour); // <-- DEĞİŞİKLİK: Parametre eklendi
    }

    private void StopPouring()
    {
        if (liquidStreamLine != null && liquidStreamLine.enabled)
        {
            liquidStreamLine.enabled = false;
        }
    }

    // Bu fonksiyon hem döken hem alan kap tarafından kullanılır
    public void UpdateShaderFill()
    {
        if (liquidMeshRenderer == null) return;

        float fillPercent = currentVolume / maxVolume;

        liquidMeshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(fillPropertyID, fillPercent);
        liquidMeshRenderer.SetPropertyBlock(propBlock);
    }

    // ---- YENİ FONKSİYON ----
    // Bu fonksiyonu SADECE alıcı kaplar kullanır
    public void AddLiquid(float amount)
    {
        currentVolume += amount;
        currentVolume = Mathf.Min(currentVolume, maxVolume); // Maksimum hacmi geçmesin
        UpdateShaderFill();
    }
    // ------------------------

    // ---- GÜNCELLENMİŞ FONKSİYON ----
    private void UpdateLineRendererStream(float amountToPour) // <-- DEĞİŞİKLİK: Parametre eklendi
    {
        Vector3 startPoint = pourOriginTransform.position;
        Vector3 initialVelocity = rb.linearVelocity + (pourOriginTransform.forward * streamVelocity);

        liquidStreamLine.positionCount = streamResolution;
        Vector3[] points = new Vector3[streamResolution];
        
        Vector3 currentPosition = startPoint;
        float timeStep = 0.05f;

        for (int i = 0; i < streamResolution; i++)
        {
            points[i] = currentPosition;

            initialVelocity += Physics.gravity * timeStep;
            Vector3 nextPosition = currentPosition + initialVelocity * timeStep;

            RaycastHit hit;
            if (Physics.Linecast(currentPosition, nextPosition, out hit, pourTargetLayer))
            {
                liquidStreamLine.positionCount = i + 1;
                points[i] = hit.point;

                // ---- YENİ KOD BLOĞU ----
                // Çarptığımız objenin 'LiquidController' script'i var mı?
                LiquidController receiver = hit.collider.GetComponent<LiquidController>();
                if (receiver != null && receiver != this) // Kendimize dökmediğimizden emin ol
                {
                    // Evet var! O zaman ona sıvı ekle.
                    receiver.AddLiquid(amountToPour);
                }
                // ------------------------
                
                break; 
            }

            currentPosition = nextPosition;
        }

        liquidStreamLine.SetPositions(points);
    }
}