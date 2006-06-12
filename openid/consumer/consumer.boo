namespace Janrain.OpenId.Consumer

import System
#import System.Collections
import System.Collections.Specialized
#import System.Text
#import System.Net
#import System.IO
#import System.Security.Cryptography
import System.Web.SessionState
import Janrain.OpenId
import Janrain.OpenId.Store


class FailureException(ApplicationException):
    identity_url as Uri
    
    def constructor(identity_url as Uri, message as string):
        super(message)
        self.identity_url = identity_url


class CancelException(ApplicationException):
    identity_url as Uri
    
    def constructor(identity_url as Uri):
        super()
        self.identity_url = identity_url


class SetupNeededException(ApplicationException):
    [Getter(ConsumerId)]
    consumer_id as Uri

    [Getter(UserSetupUrl)]
    user_setup_url as Uri
    
    def constructor(consumer_id as Uri, user_setup_url as Uri):
        super()
        self.consumer_id = consumer_id
        self.user_setup_url = user_setup_url


class Consumer:
    session as HttpSessionState
    consumer as GenericConsumer

    [Property(SessionKeyPrefix)]
    private session_key_prefix = "_openid_consumer_"

    private last_token = "last_token"

    protected TokenKey:
        get:
            return session_key_prefix + last_token

    manager as ServiceEndpointManager

    def constructor(session as HttpSessionState, store as IAssociationStore):
        self.session = session
        self.manager = ServiceEndpointManager(session)
        self.consumer = GenericConsumer(store, SimpleFetcher())
    
    def Begin(openid_url as Uri):
        endpoint = self.manager.GetNextService(openid_url,
                                               self.SessionKeyPrefix)
        if endpoint is null:
            raise FailureException(null, 'No openid endpoint found')
        return BeginWithoutDiscovery(endpoint)

    def BeginWithoutDiscovery(endpoint as ServiceEndpoint):
        auth_req = self.consumer.Begin(endpoint)
        self.session[self.TokenKey] = auth_req.Token
        return auth_req

    def Complete(query as NameValueCollection):
        token = self.session[TokenKey]
        if token is null:
            raise FailureException(null, 'No session state found')

        response = self.consumer.Complete(query, token)
        self.manager.Cleanup(response.IdentityUrl, self.TokenKey)
            
        return response
