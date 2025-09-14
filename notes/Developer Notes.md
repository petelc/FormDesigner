# Developer Notes

### Update Form

Currently the update to a form is handled in the UseCases Project using the UpdateFormCommand and the UpdateFormHandler. But the delete form is handled in the Core project as a service. What would it take to handle the update form as a service in the Core project similiar to the delete form service is?

What do I need to do:

**\_\_** Create the IFormUpdateService in the interfaces namespace.
**\_\_** Create the FormUpdateEvent in the Events namespace
**\_\_** Create the FormUpdateService in the Services namespace
**\_\_** Implement the IFormUpdateService interface in the FormUpdateService class
**\_\_** Create the FormUpdatedHandler in the Handlers namespace

In the UseCases Project:
**\_\_\_** Update the handler to call the service from the Core project.

### Registering Update Form Service

~~Currently the FormUpdateService is registered in the Program class of the Web project. I should move it to the Infrastructure Service Extensions.~~

Ok rethinking this, I am going to move the update form service registration to the Services Config.
