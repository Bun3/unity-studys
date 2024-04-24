using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject unit;

    [SerializeField] private int cnt = 10000;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        for (var i_ = 0; i_ < cnt; i_++)
        {
            Instantiate(unit, new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0), Quaternion.identity);

            if (i_ % 1000 == 0)
                yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
