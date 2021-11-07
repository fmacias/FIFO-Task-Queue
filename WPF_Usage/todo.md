# Todo. Create abastract components, whose define one AbstractModul
## DAL
Couple with a "in memory Database"

## Repository
### Create one IRepository.
### Resolve Agnostic Adpter implementation, wich access the DAL
### Couple Adapter

## BLL
### Resolve respository agnostic implementation
### Agnostic implementaton without functionality and with one dependency by default

## Presenter
### Resolve agnostic IViewModel
### Resolve EventAgregator
#### Define the Abstract Subscribe method
### Resolve View(Generic) and cast at concrete implementation
### abstract class

## ViewModel
### Implement INotifiedPropertyChanged
### abstract class

## Model(DomainModel)
### Just hast to be present withous implementation(Interface)

## Module
### Create agostic implemetnation without dependency to unity to resolve.
So that the module it self will not have any concrete dependency to any concrete IoC container.
### Create a concrete implemtation of this adapter coupled to unity.
