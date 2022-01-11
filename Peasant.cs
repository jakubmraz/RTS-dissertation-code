using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Peasant : Unit
{
    [SerializeField] private GameObject peasantsAxe;
    [SerializeField] private GameObject peasantsBag;
    [SerializeField] private GameObject peasantsWood;

    [HideInInspector] public int goldCarried;
    [HideInInspector] public int lumberCarried;
    public int maxGoldCarried;
    public int maxLumberCarried;

    [HideInInspector] public bool harvestingGold;
    [HideInInspector] public bool inMine;
    [HideInInspector] public bool harvestingLumber;
    private GoldMine lastGoldMine;
    private Building queuedBuilding;
    [HideInInspector] public Vector3 lastTreeLocation;

    public override void ResetSate()
    {
        base.ResetSate();
        if (queuedBuilding != null)
        {
            GridBuildingSystem.Instance.DestroyPlacedBuilding(queuedBuilding);
            queuedBuilding = null;
        }
    }

    public override void Stop()
    {
        if(!inMine) ResetSate();

        currentState = EntityState.Defending;

        UpdateAnimation();
    }

    public void GoHarvestGold(GoldMine goldMine)
    {
        MoveToLocation(goldMine.transform.position);
        harvestingGold = true;
    }

    public void GoHarvestLumber(Vector3 treeLocation)
    {
        AttackMove(treeLocation);
        harvestingLumber = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gold Mine") && harvestingGold && goldCarried < maxGoldCarried)
        {
            StartCoroutine(MiningCoroutine(other.gameObject.GetComponent<GoldMine>()));
            return;
        }

        if (other.CompareTag("Tree") && harvestingLumber && lumberCarried < maxLumberCarried)
        {
            Attack(other.GetComponent<Trees>());
            lastTreeLocation = other.gameObject.transform.position;
            return;
        }

        if (other.CompareTag("Town Hall") && (goldCarried > 0 || lumberCarried > 0))
        {
            team.gold += goldCarried;
            team.lumber += lumberCarried;
            goldCarried = 0;
            lumberCarried = 0;
            if (harvestingGold && lastGoldMine != null && lastGoldMine.CurrentGold > 0)
            {
                GoHarvestGold(lastGoldMine);
                lastGoldMine = null;
            }
            else if (harvestingLumber && lastTreeLocation != Vector3.zero)
            {
                GoHarvestLumber(lastTreeLocation);
                lastTreeLocation = Vector3.zero;
            }
            else
            {
                navMeshAgent.ResetPath();
                harvestingGold = false;
                harvestingLumber = false;
            }
            return;
        }

        if (other.GetComponent<Building>() != null && other.GetComponent<Building>() == queuedBuilding)
        {
            Debug.Log("yes.");
            StartBuilding(queuedBuilding);
        }
    }

    private IEnumerator MiningCoroutine(GoldMine goldMine)
    {
        if (goldMine.CurrentGold > 0)
        {
            Vector3 oldLocation = transform.position;
            inMine = true;
            navMeshAgent.Warp(new Vector3(0, 0, 0));
            canMove = false;
            
            yield return new WaitForSeconds(5f);

            navMeshAgent.Warp(oldLocation);
            canMove = true;
            goldCarried = goldMine.MineGold(maxGoldCarried);
            lastGoldMine = goldMine;
            inMine = false;
            ReturnResources();
        }
    }

    public void ReturnResources()
    {
        if (goldCarried > 0 || lumberCarried > 0)
        {
            currentState = EntityState.Moving;
            UpdateAnimation();
            List<TownHall> townHalls = new List<TownHall>();
            foreach (var building in team.GetBuildings())
            {
                if (building is TownHall townHall)
                    townHalls.Add(townHall);
            }
            TownHall nearestTownHall = townHalls.OrderBy(t => (t.transform.position - this.transform.position).sqrMagnitude)
                .FirstOrDefault();
            if(nearestTownHall != null)
                navMeshAgent.SetDestination(nearestTownHall.transform.position);
            else
                Debug.Log("No Town Hall found.");
        }
        //If only carrying lumber, look for lumber mill first
        else
            Debug.Log("No resources to return.");
    }

    public override void OnMove()
    {
        base.OnMove();
        harvestingGold = false;
        harvestingLumber = false;
    }

    public override void OnAttackSuccessful(Entity target)
    {
        base.OnAttackSuccessful(target);
        lumberCarried++;
        if (lumberCarried >= maxLumberCarried)
        {
            ReturnResources();
            if (lumberCarried > maxLumberCarried)
                lumberCarried = maxLumberCarried;
        }
    }

    public override void OnCommanded()
    {
        base.OnCommanded();
        Stop();
        harvestingLumber = false;
        harvestingGold = false;
    }

    public override void UpdateAnimation()
    {
        base.UpdateAnimation();
        animator.SetBool("attacking", false);
        if (currentState == EntityState.Attacking)
        {
            peasantsAxe.gameObject.SetActive(true);
            animator.SetBool("attacking", true);
            animator.SetFloat("attackSpeed", baseAttackTime / 2.267f + 1);
        }
        else
        {
            peasantsAxe.gameObject.SetActive(false);
            animator.SetBool("attacking", false);
        }
        if(goldCarried > 0)
            peasantsBag.gameObject.SetActive(true);
        else
            peasantsBag.gameObject.SetActive(false);
        if(lumberCarried > 0)
            peasantsWood.gameObject.SetActive(true);
        else
            peasantsWood.gameObject.SetActive(false);
    }

    public void GoBuildBuilding(Building building)
    {
        MoveToLocation(building.visual.GetComponentInChildren<Renderer>().bounds.center);
        queuedBuilding = building;
    }

    private void StartBuilding(Building building)
    {
        building.ActivateBuilding(this);
        queuedBuilding = null;
        Stop();
    }
}
