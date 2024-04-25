using UnityEditor;
using UnityEngine;

class CreateSpawnpoints : ScriptableWizard
{

    public GameObject spawnPoint;
    public Transform container;
    
    [MenuItem("SpawnPoints/Create Points by Selection...")]
    static void SelectAllOfTagWizard()
    {
        DisplayWizard(
            "Create Points by Selection...",
            typeof(CreateSpawnpoints),
            "Create");
    }

    void OnWizardCreate()
    {
        
        foreach(GameObject obj in Selection.objects)
        {
            Quaternion rotation = Quaternion.Euler(
                spawnPoint.transform.rotation.eulerAngles.x,
                obj.transform.rotation.eulerAngles.y,
                spawnPoint.transform.rotation.eulerAngles.z);
            
            Vector3 pos = obj.transform.position;

            if (Mathf.Approximately(rotation.eulerAngles.y, 270))
            {
                pos.x -= 10;
            }
            else if (Mathf.Approximately(rotation.eulerAngles.y, 90))
            {
                pos.x += 10;
            }
            else if (Mathf.Approximately(Mathf.Abs(rotation.eulerAngles.y), 180))
            {
                pos.z -= 10;
            }
            else
            {
                pos.z += 10;
            }

            pos.y += 1;
            Debug.Log(rotation.eulerAngles.y);
            
            Instantiate(spawnPoint, pos, spawnPoint.transform.rotation, container);
            
        }
    }
}

