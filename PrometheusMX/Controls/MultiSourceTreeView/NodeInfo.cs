using System;
using System.Collections.Generic;
using System.Drawing;

namespace Prometheus.Controls
{
  /// <summary>
  /// Represents a NodeSource data item that corresponds to a MultiSourceTreeNode.
  /// </summary>
  public class NodeInfo : IDisposable
  {
    private MultiSourceTreeNode node;
    private NodeType nodeType;
    private string identifier;
    private List<NodeStateSubscriber> stateSubscriptions = new List<NodeStateSubscriber>();
    private bool disposed = false;
    private object tag;

    /// <summary>
    /// A custom object that can be persisted along with the standard identifier.
    /// </summary>
    public object Tag
    {
      get { return tag; }
      set { tag = value; }
    }

    /// <summary>
    /// Gets or sets the Node that this NodeInfo belongs to.
    /// </summary>
    public MultiSourceTreeNode Node
    {
      get { return node; }
      set { node = value; }
    }

    /// <summary>
    /// gets the NodeType that this NodeInfo represents.
    /// </summary>
    public NodeType NodeType
    {
      get { return nodeType; }
    }

    /// <summary>
    /// Gets or sets the identifier that represents this Node in it's NodeSource.
    /// </summary>
    public string Identifier
    {
      get { return identifier; }
      set { identifier = value; }
    }

    /// <summary>
    /// Returns the highest priority active NodeState.
    /// </summary>
    public NodeState PriorityState
    {
      get
      {
        for (int x = stateSubscriptions.Count - 1; x > 0; x--)
          if (stateSubscriptions[x].Active)
            return stateSubscriptions[x].State;

        return stateSubscriptions[0].State; // The default state should *always* be active.
      }
    }

    /// <summary>
    /// Gets a list of NodeStates that this NodeInfo is subscribed to.
    /// </summary>
    public NodeStateSubscriber[] Subscriptions
    {
      get { return stateSubscriptions.ToArray(); }
    }

    /// <summary>
    /// Gets the NodeSource that contains the identifier represented by this node.
    /// </summary>
    public NodeSource ParentSource
    {
      get { return nodeType.ParentSource; }
    }

    /// <summary>
    /// Creates a new instance of the NodeInfo class with the specified values.
    /// </summary>
    public NodeInfo(NodeType nodeType, string identifier)
    {
      this.nodeType = nodeType;
      this.identifier = identifier;
      foreach (NodeState state in nodeType.States)
        Subscribe(state.GetSubscription(this));
    }

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    public void Dispose()
    {
      if (!disposed)
        foreach (NodeStateSubscriber subscriber in stateSubscriptions)
          subscriber.StateChanged -= new EventHandler<NodeStateChangedArgs>(StateChanged);
    }
    
    /// <summary>
    /// Handles StateChanged events generated by the NodeInfo's NodeStateModifiers.
    /// </summary>
    private void StateChanged(object sender, NodeStateChangedArgs e)
    {
      Update();           
    }

    /// <summary>
    /// Updates the Node represented by this NodeInfo.
    /// </summary>
    public void Update()
    {
      NodeType.ParentSource.UpdateNode(node);
    }

    public void Subscribe(NodeStateSubscriber subscriber)
    {
      stateSubscriptions.Add(subscriber);
      subscriber.StateChanged += new EventHandler<NodeStateChangedArgs>(StateChanged);
    }

    public MultiSourceTreeNode[] BuildChildNodes()
    {
      return ParentSource.BuildChildNodes(nodeType, Identifier);
    }
  }
}