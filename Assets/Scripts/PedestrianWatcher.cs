using UnityEngine;
using UniRx;
using System.Linq;

public class PedestrianWatcher : MonoBehaviour {

    Datastore datastore;

    public void Start() {
        datastore = this.GetComponent<Datastore>();

        datastore.gameEvents.Receive<PedestrianSpawnedEvent>().Subscribe(e => {
            datastore.allPedestrians.AddRange(e.pedestrians);
            datastore.totalPopulation.Value = datastore.allPedestrians.Count;
        });

        datastore.gameEvents.Receive<PedestrianTripCompletedEvent>().Subscribe(_ => datastore.completedTrips.Value++);

        datastore.tickCounter.Subscribe(e => {
            var pedestriansToDespawn = datastore.allPedestrians
                .Where(ped => ped.currentPatience < 0).ToList();
            datastore.allPedestrians = datastore.allPedestrians.Except(pedestriansToDespawn).ToList();
            datastore.totalPopulation.Value = datastore.allPedestrians.Count;
            datastore.gameEvents.Publish<PedestrianDespawnedEvent>(new PedestrianDespawnedEvent() {
                pedestrians = pedestriansToDespawn,
            });
        });
    }
}