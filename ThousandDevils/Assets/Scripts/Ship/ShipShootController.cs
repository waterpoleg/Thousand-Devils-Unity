using System;
using System.Collections.Generic;
using Assets.Scripts.Cannonball;
using Assets.Scripts.ObjectPoolingManager;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.Ship
{
    public class ShipShootController : MonoBehaviour
    {
        [SerializeField] private float _speed;
        
        [SerializeField] private int _minShootingAccuracy = 0;
        [SerializeField] private int _maxShootingAccuracy = 100;

        [SerializeField] private int _aimingSpeed = 30;

        public List<GameObject> Cannons;

        private float _shootingAccuracy;

        private Quaternion[] _cannonRotations;

        void Awake()
        {
            // remember initial cannon angles
            _cannonRotations = (from cannon in Cannons select cannon.transform.localRotation).ToArray();
        }

        public void Shoot()
        {
            var charger = gameObject.GetComponent<Charger>();

            if (charger.IsReadyToShoot() == false)
                return;

            foreach (var cannon in Cannons)
            {
                var cannonBall = PoolManager.GetInstance().PlacePooledObject("CannonBall", cannon.transform.position, cannon.transform.rotation);
                cannonBall.GetComponent<CannonballMovementController>().ChangeSpeed(_speed);
            }

            charger.StartCharge();
        }

        public void IncreaseShootingAccuracy()
        {
            _shootingAccuracy += _aimingSpeed * Time.deltaTime;
            _shootingAccuracy = Mathf.Clamp(_shootingAccuracy, _minShootingAccuracy, _maxShootingAccuracy);

            for (int i = 0; i < Cannons.Count; i++)
            {
                var shootingDirection = Quaternion.Euler(Cannons[i].transform.localRotation.eulerAngles.x,
                    Cannons[i].transform.localRotation.eulerAngles.y, ApplyAccuracy(i));

                Cannons[i].transform.localRotation = shootingDirection;
            }
        }

        public void ResetShootingAccuracy()
        {
            _shootingAccuracy = _minShootingAccuracy;

            for (int i = 0; i < Cannons.Count; i++)
            {
                Cannons[i].transform.localRotation = _cannonRotations[i];
            }
        }

        float ApplyAccuracy(int cannonNumber)
        {
            var angle = _cannonRotations[cannonNumber].eulerAngles.z;

            // always between -180 and 180
            while (angle > 180)
            {
                angle -= 360;
            }

            while (angle < -180)
            {
                angle += 360;
            }

            var sign = Mathf.Sign(angle);

            var minAccuracyAngle = angle;
            var maxAccuracyAngle = sign * 90;
            // always between 0 and 1
            var normalizedAccuracy = (_shootingAccuracy - _minShootingAccuracy) / (_maxShootingAccuracy - _minShootingAccuracy);
            var newAngle = Mathf.Lerp(minAccuracyAngle, maxAccuracyAngle, normalizedAccuracy);

            return newAngle;
        }
    }
}