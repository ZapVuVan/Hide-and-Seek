//// WeaponController.cs
//using UnityEngine;

//public class WeaponController : MonoBehaviour
//{
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private string bulletTag = "Bullet";
//    [SerializeField] private Camera cam;

//    public void Shoot()
//    {
//        //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
//        //Vector3 targetPoint;

//        //if (Physics.Raycast(ray, out RaycastHit hit))
//        //    targetPoint = hit.point;
//        //else
//        //    targetPoint = ray.GetPoint(50f);

//        //GameObject obj = ObjectPool.Instance.Get(
//        //    bulletTag,
//        //    firePoint.position,
//        //    Quaternion.identity
//        //);

//        //if (obj != null && obj.TryGetComponent<Bullet>(out var bullet))
//        //    bullet.Init(bulletTag, targetPoint);

//        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
//        if(Physics.Raycast(ray, out RaycastHit hit))
//        {
//            Debug.Log($"Chạm vào: {hit.collider.gameObject.name} | Vị trí: {hit.point}");
//            if(hit.collider.TryGetComponent<BotController>(out var bot))
//            {

//            }
//        }
//    }
//}