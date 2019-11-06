using UnityEngine;

namespace TrafficParticipant
{
    [RequireComponent(typeof(ContinuousLaneController))]
    public partial class VehicleLaneController : MonoBehaviour
	{
		public void RightSwitchLane()
		{
			if (laneOn == linkOn.maxLane)
			{
				return;
			}

			if (leftFollower)
			{
				if (!directFollowing)
				{
					leftFollower.rightFollowing = null;
				}
				else
				{
					if (leftFollower.directFollowing == null ||
						directFollowing.isRightNeighborOf(leftFollower.directFollowing, out bool isBehind, out _) && isBehind)
					{
						leftFollower.rightFollowing = directFollowing;
					}
					else
					{
						leftFollower.rightFollowing = null;
					}
				}
			}

			// No more relationship with left follower
			if (directFollower && directFollower.leftFollowing == null)
			{
				directFollower.leftFollowing = leftFollowing;
			}

			laneOn++;
			if (rightFollowing && directFollowing)
			{
				if (directFollowing.isLeftNeighborOf(rightFollowing, out bool isBehind, out _) && isBehind)
				{
					leftFollowing = directFollowing;
				}
				else
				{
					leftFollowing = null;
				}
			}
			else
			{
				leftFollowing = directFollowing;
			}
			// No more relationship with left following

			if (directFollower && directFollower.rightFollowing == null)
			{
				directFollower.rightFollowing = this;
			}

			if (directFollower)
			{
				directFollower.directFollowing = directFollowing;
			}

			if (directFollowing)
			{
				directFollowing.directFollower = directFollower;
			}
			// No more relationship with original directFollower/ing

			// Look for new directFollower
			if (rightFollower)
			{
				directFollower = rightFollower;
				rightFollower.leftFollowing = null;
			}
			else
			{
				if (rightFollowing)
				{
					directFollower = rightFollowing.directFollower;
				}
				else
				{
					directFollower = _SearchFollower(linkOn, laneOn, laneOn, percentageTravelled);
				}
			}


			// Look for new directFollowing
			if (rightFollowing)
			{
				directFollowing = rightFollowing;
			}
			else
			{
				if (directFollower)
				{
					directFollowing = directFollower.directFollowing;
				}
				else
				{
					directFollowing = _SearchFollowing(linkOn, laneOn, laneOn, percentageTravelled);
				}
			}

			if (directFollower)
			{
				directFollower.directFollowing = this;
			}
			if (directFollowing)
			{
				directFollowing.directFollower = this;
			}

			if (laneOn == linkOn.maxLane)
			{
				rightFollowing = null;
				return;
			}

			// Look for rightFollower
			if (directFollower)
			{
				if (directFollower.rightFollowing)
				{
					if (directFollower.rightFollowing.isRightNeighborOf(this, out bool behind, out _))
					{
						if (!behind)
						{
							// No rightFollower
							directFollower.rightFollowing = null;
						}
						else
						{
							var backMost = directFollower.rightFollowing;
							while (backMost.directFollowing && backMost.directFollowing.isRightNeighborOf(this, out bool behind2, out _) && behind2)
							{
								backMost = backMost.directFollowing;
							}
							backMost.leftFollowing = this;
						}
					}
					else
					{
						var candidate = _SearchFollower(linkOn, laneOn + 1, laneOn, percentageTravelled);
						if (candidate)
						{
							candidate.leftFollowing = this;
						}
					}
				}
				else
				{
					var candidate = _SearchFollower(linkOn, laneOn + 1, laneOn, percentageTravelled);
					if (candidate && !candidate.isRightNeighborOf(directFollower, out _, out _))
					{
						candidate.leftFollowing = this;
					}
					else
					{
						// very special case: directFollower has diverted
					}
				}
			}
			else
			{
				var candidate = _SearchFollower(linkOn, laneOn + 1, laneOn, percentageTravelled);
				if (candidate)
				{
					candidate.leftFollowing = this;
				}
			}

			if (directFollowing)
			{
				if (directFollowing.rightFollower)
				{
					if (directFollowing.rightFollower.isRightNeighborOf(this, out bool behind, out _))
					{
						if (behind)
						{
							rightFollowing = null;
							directFollowing.rightFollower.leftFollowing = this;
						}
						else
						{
							var frontMost = directFollowing.rightFollower;
							while (frontMost.directFollower && frontMost.directFollower.isRightNeighborOf(this, out bool behind2, out _) && !behind2)
							{
								frontMost = frontMost.directFollower;
							}
							rightFollowing = frontMost;
						}
					}
					else
					{
						rightFollowing = _SearchFollowing(linkOn, laneOn + 1, laneOn, percentageTravelled);
					}
				}
				else
				{
					var candidate = _SearchFollowing(linkOn, laneOn + 1, laneOn, percentageTravelled);
					if (candidate && candidate.isRightNeighborOf(directFollowing, out _, out _))
					{
						rightFollowing = null;
					}
					else
					{
						// very special case: directFollowing has diverted
						rightFollowing = candidate;
					}
				}
			}
			else
			{
				rightFollowing = _SearchFollowing(linkOn, laneOn + 1, laneOn, percentageTravelled);
			}

		}

		public void LeftSwitchLane()
		{
            if (laneOn == linkOn.minLane)
			{
				return;
			}

            if (rightFollower)
            {
                if (!directFollowing)
                {
                    rightFollower.leftFollowing = null;
                }
                else
                {
                    if (rightFollower.directFollowing == null ||
                            directFollowing.isLeftNeighborOf(rightFollower.directFollowing, out bool isBehind, out _) && isBehind)
                    {
                        rightFollower.leftFollowing = directFollowing;
                    }
                    else
                    {
                        rightFollower.leftFollowing = null;
                    }
                }
            }

            // No more relationship with right follower
            if (directFollower && directFollower.rightFollowing == null)
            {
                directFollower.rightFollowing = rightFollowing;
            }

            laneOn--;

            if (leftFollowing && directFollowing)
            {
                if (directFollowing.isRightNeighborOf(leftFollowing, out bool isBehind, out _) && isBehind)
                {
                    rightFollowing = directFollowing;
                }
                else
                {
                    rightFollowing = null;
                }
            }
            else
            {
                rightFollowing = directFollowing;
            }
            // No more relationship with right following

            if (directFollower && directFollower.leftFollowing == null)
            {
                directFollower.leftFollowing = this;
            }

            if (directFollower)
            {
                directFollower.directFollowing = directFollowing;
            }

            if (directFollowing)
            {
                directFollowing.directFollower = directFollower;
            }

            // No more relationship with original directFollower/ing

            // Look for new directFollower
            if (leftFollower)
            {
                directFollower = leftFollower;
                leftFollower.rightFollowing = null;
            }
            else
            {
                if (leftFollowing)
                {
                    directFollower = leftFollowing.directFollower;
                }
                else
                {
                    directFollower = _SearchFollower(linkOn, laneOn, laneOn, percentageTravelled);
                }
            }

            // Look for new directFollowing
            if (leftFollowing)
            {
                directFollowing = leftFollowing;
            }
            else
            {
                if (directFollower)
                {
                    directFollowing = directFollower.directFollowing;
                }
                else
                {
                    directFollowing = _SearchFollowing(linkOn, laneOn, laneOn, percentageTravelled);
                }
            }

            if (directFollower)
            {
                directFollower.directFollowing = this;
            }
            if (directFollowing)
            {
                directFollowing.directFollower = this;
            }

            if (laneOn == linkOn.minLane)
            {
                leftFollowing = null;
                return;
            }

            // Look for leftFollower
            if (directFollower)
            {
                if (directFollower.leftFollowing)
                {
                    if (directFollower.leftFollowing.isLeftNeighborOf(this, out bool behind, out _))
                    {
                        if (!behind)
                        {
                            // no leftFollower
                            directFollower.leftFollowing = null;
                        }
                        else
                        {
                            var backMost = directFollower.leftFollowing;
                            while (backMost.directFollowing && backMost.directFollowing.isLeftNeighborOf(this, out bool behind2, out _) && behind2)
                            {
                                backMost = backMost.directFollowing;
                            }
                            backMost.rightFollowing = this;
                        }
                    }
                    else
                    {
                        var candidate = _SearchFollower(linkOn, laneOn - 1, laneOn, percentageTravelled);
                        if (candidate)
                        {
                            candidate.rightFollowing = this;
                        }
                    }
                }
                else
                {
                    var candidate = _SearchFollower(linkOn, laneOn - 1, laneOn, percentageTravelled);
                    if (candidate && !candidate.isLeftNeighborOf(directFollower, out _, out _))
                    {
                        candidate.rightFollowing = this;
                    }
                    else
                    {
                        // very special case: directFolloweer has diverted
                    }
                }
            }
            else
            {
                var candidate = _SearchFollower(linkOn, laneOn - 1, laneOn, percentageTravelled);
                if (candidate)
                {
                    candidate.rightFollowing = this;
                }
            }

            if (directFollowing)
            {
                if (directFollowing.leftFollower)
                {
                    if (directFollowing.leftFollower.isLeftNeighborOf(this, out bool behind, out _))
                    {
                        if (behind)
                        {
                            leftFollowing = null;
                            directFollowing.leftFollower.rightFollowing = this;
                        }
                        else
                        {
                            var frontMost = directFollowing.leftFollower;
                            while (frontMost.directFollower && frontMost.directFollower.isLeftNeighborOf(this, out bool behind2, out _) && !behind2)
                            {
                                frontMost = frontMost.directFollower;
                            }
                            leftFollowing = frontMost;
                        }
                    }
                    else
                    {
                        leftFollowing = _SearchFollowing(linkOn, laneOn - 1, laneOn, percentageTravelled);
                    }
                }
                else
                {
                    var candidate = _SearchFollowing(linkOn, laneOn - 1, laneOn, percentageTravelled);
                    if (candidate && candidate.isLeftNeighborOf(directFollowing, out _, out _))
                    {
                        leftFollowing = null;
                    }
                    else
                    {
                        // very special case: directFollowing has diverted
                        leftFollowing = candidate;
                    }
                }
            }
            else
            {
                leftFollowing = _SearchFollowing(linkOn, laneOn - 1, laneOn, percentageTravelled);
            }
        }
    }
}
