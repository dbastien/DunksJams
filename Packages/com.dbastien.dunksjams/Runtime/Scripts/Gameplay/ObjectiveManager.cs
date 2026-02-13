using System;
using System.Collections.Generic;

//todo: largely untested
public class ObjectiveManager
{
    public enum ObjectiveStatus
    {
        InProgress,
        Completed,
        Failed
    }

    public class Objective
    {
        public string Title { get; }
        public string Description { get; }
        public ObjectiveStatus Status { get; private set; }

        public event Action<Objective> OnStatusChanged;

        public Objective(string title, string description)
        {
            Title = title;
            Description = description;
            Status = ObjectiveStatus.InProgress;
        }

        public void Complete()
        {
            if (Status != ObjectiveStatus.InProgress) return;
            Status = ObjectiveStatus.Completed;
            OnStatusChanged?.Invoke(this);
        }

        public void Fail()
        {
            if (Status != ObjectiveStatus.InProgress) return;
            Status = ObjectiveStatus.Failed;
            OnStatusChanged?.Invoke(this);
        }

        public void Reset()
        {
            Status = ObjectiveStatus.InProgress;
            OnStatusChanged?.Invoke(this);
        }
    }

    readonly HashSet<Objective> _objectives = new();
    public IEnumerable<Objective> Objectives => _objectives;

    public event Action<Objective> OnObjectiveCompleted, OnObjectiveFailed;
    public event Action OnAllObjectivesCompleted;

    public void AddObjective(Objective objective)
    {
        if (objective == null) throw new ArgumentNullException(nameof(objective));

        if (_objectives.Add(objective))
            objective.OnStatusChanged += HandleObjectiveStatusChanged;
    }

    public void RemoveObjective(Objective objective)
    {
        if (_objectives.Remove(objective))
            objective.OnStatusChanged -= HandleObjectiveStatusChanged;
    }

    public void CheckAllObjectives()
    {
        foreach (var objective in _objectives)
        {
            if (objective.Status != ObjectiveStatus.Completed)
                return;
        }

        OnAllObjectivesCompleted?.Invoke();
    }

    public void ResetAllObjectives()
    {
        foreach (var objective in _objectives) objective.Reset();
    }

    void HandleObjectiveStatusChanged(Objective objective)
    {
        switch (objective.Status)
        {
            case ObjectiveStatus.Completed:
                OnObjectiveCompleted?.Invoke(objective);
                break;
            case ObjectiveStatus.Failed:
                OnObjectiveFailed?.Invoke(objective);
                break;
        }

        CheckAllObjectives();
    }
}