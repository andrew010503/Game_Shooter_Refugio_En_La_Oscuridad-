using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Animations.Rigging;
using Cinemachine;

public class WeaponHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera cm_camera;
    [SerializeField] private Transform muzzlePoint;
    private Animator anim; 
    private ThirdPersonController controller;
    private Cinemachine3rdPersonFollow thirdPersonFollow;
    private Camera mainCamera;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 0.09f;
    [SerializeField] private float shootBlendTime = 0.075f;
    [SerializeField] private string shootStateName = "Fire_Rifle";
    [SerializeField] private AudioClip shootSound;
    [SerializeField] [Range(0f, 1f)] private float shootSoundVolume = 0.3f;
    [SerializeField] private ParticleSystem muzzleFlash;
    private bool canShoot = true;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpawnOffset = 0.5f;

    [Header("Aiming")]
    [SerializeField] private float cameraTranslationSpeed = 7f;
    [SerializeField] private float ikTransitionSpeed = 10f;
    [SerializeField] private MultiAimConstraint aimIK;
    [Space(10)]
    [SerializeField] private float aimCameraSide = 0.75f;
    [SerializeField] private float aimCameraDistance = 0.5f;
    private float defaultCameraSide;
    private float defaultCameraDistance;

    public bool Aiming { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject crosshair;

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<ThirdPersonController>();
        mainCamera = Camera.main;
        
        if (cm_camera != null)
        {
            thirdPersonFollow = cm_camera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            
            if (thirdPersonFollow != null)
            {
                defaultCameraSide = thirdPersonFollow.CameraSide;
                defaultCameraDistance = thirdPersonFollow.CameraDistance;
            }
        }
    }

    private void Update()
    {
        Aiming = Input.GetButton("Fire2");
        bool shootInput = Input.GetButton("Fire1");

        anim.SetBool("Aiming", Aiming);
        controller.Strafe = true;

        if (Aiming && shootInput)
        {
            Shoot();
        }

        if (thirdPersonFollow != null)
        {
            float targetSide = Aiming ? aimCameraSide : defaultCameraSide;
            float targetDistance = Aiming ? aimCameraDistance : defaultCameraDistance;

            thirdPersonFollow.CameraSide = Mathf.Lerp(thirdPersonFollow.CameraSide, targetSide, cameraTranslationSpeed * Time.deltaTime);
            thirdPersonFollow.CameraDistance = Mathf.Lerp(thirdPersonFollow.CameraDistance, targetDistance, cameraTranslationSpeed * Time.deltaTime);
        }

        if (crosshair != null)
        {
            crosshair.SetActive(Aiming);
        }

        if (aimIK != null)
        {
            float targetWeight = Aiming ? 1f : 0f;
            aimIK.weight = Mathf.Lerp(aimIK.weight, targetWeight, ikTransitionSpeed * Time.deltaTime);
        }
    }

    private void Shoot()
    {
        if (!canShoot)
            return;

        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, shootSoundVolume);
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (anim != null)
        {
            anim.CrossFadeInFixedTime(shootStateName, shootBlendTime);
        }

        SpawnProjectile();

        StartCoroutine(ResetFireRate());
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null || muzzlePoint == null || mainCamera == null)
            return;

        Vector3 shootDirection = mainCamera.transform.forward;
        Vector3 spawnPosition = muzzlePoint.position + shootDirection * projectileSpawnOffset;

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
    }

    private IEnumerator ResetFireRate()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
}

