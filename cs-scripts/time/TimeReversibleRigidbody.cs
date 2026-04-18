using UnityEngine;
using System.Collections;
using CommandPattern;

namespace Assets.Projects.Command
{
	public class TimeReversibleRigidbody: TimeReversible
	{
        private Rigidbody rb;
        private MeshRenderer mesh;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            mesh = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            rb.isKinematic = isReversing;
            mesh.material.SetFloat("_isReversing", isReversing ? 1f : 0f);
        }
    }
}