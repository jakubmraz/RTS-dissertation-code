using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldUI;
    [SerializeField] private TextMeshProUGUI lumberUI;
    [SerializeField] private TextMeshProUGUI foodUI;

    [SerializeField] private TextMeshProUGUI nameUI;
    [SerializeField] private RawImage portrait;
    [SerializeField] private GameObject portraitSpot;
    [SerializeField] private List<CommandHolder> commands;

    public void UpdateTopUI(Team team)
    {
        goldUI.text = "" + team.gold;
        lumberUI.text = "" + team.lumber;
        foodUI.text = team.GetFoodConsumed() + "/" + team.GetFoodMax();
    }

    public void UpdateBottomUI(Entity entity)
    {
        nameUI.text = entity.entityName;
        //if (entity is ActiveEntity activeEntity)
        //    portrait.sprite = activeEntity.icon;

        FillCommands(entity);
        UpdatePortrait(entity);
    }

    public void FillCommands(Entity entity)
    {
        int i;
        FlushCommands();
        switch (entity)
        {
            case Building building:
                i = 0;
                foreach (var production in building.availableProductions)
                {
                    commands[i].command = Productions.ProductionsList.FirstOrDefault(command => command.commandName == production);
                    commands[i].GetComponent<Image>().sprite = commands[i].command.sprite;
                    i++;
                }
                break;
            case ActiveEntity active:
                //Generic commands, these are the same for every unit so I'm hardcoding them to avoid the need for Linq
                for (i = 0; i < 5; i++)
                {
                    commands[i].command = GenericCommands.CommandList[i];
                    commands[i].GetComponent<Image>().sprite = commands[i].command.sprite;
                }
                //Abilities
                foreach (var ability in active.availableAbilities)
                {
                    commands[i].command = Abilities.AbilityList.FirstOrDefault(command => command.commandName == ability);
                    commands[i].GetComponent<Image>().sprite = commands[i].command.sprite;
                    i++;
                }
                break;
        }
    }

    public void FillBuildCommands(ActiveEntity activeEntity)
    {
        FlushCommands();

        int i = 0;
        foreach (var build in activeEntity.builds)
        {
            commands[i].command = BuildingCommands.BuildingsList.FirstOrDefault(command => command.commandName == build);
            commands[i].GetComponent<Image>().sprite = commands[i].command.sprite;
            i++;
        }

        commands[commands.Count - 1].command = Abilities.AbilityList.FirstOrDefault(command => command.commandName == "Cancel");
        commands[commands.Count - 1].GetComponent<Image>().sprite = commands[commands.Count - 1].command.sprite;
    }

    private void FlushCommands()
    {
        foreach (var commandHolder in commands)
        {
            commandHolder.command = null;
            Image commandImage = commandHolder.GetComponent<Image>();
            commandImage.sprite = null;
        }
    }

    private void UpdatePortrait(Entity entity)
    {
        foreach (Transform child in portraitSpot.transform)
        {
            if(child.GetComponent<Camera>() == null) Destroy(child.gameObject);
        }

        GameObject entityPortrait = Instantiate(entity, portraitSpot.transform.position, Quaternion.identity, portraitSpot.transform).gameObject;
        foreach (var component in entityPortrait.GetComponents<Component>())
        {
            if (!(component is Transform) && !(component is Animator))
            {
                Destroy(component);
            }
            else if (component is Animator animator)
            {
                animator.SetBool("walking", false);
                animator.SetBool("attacking", false);
                animator.SetBool("attacking2", false);
            }
        }

        foreach (Transform child in entityPortrait.transform)
        {
            if(child.GetComponent<HPBar>() != null) Destroy(child.gameObject);
        }
    }
}
