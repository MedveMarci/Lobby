using System;
using System.Collections.Generic;
using System.Linq;
using MapGeneration;
using UnityEngine;

namespace Lobby.API;

public static class LobbyLocationHandler
{
    public static GameObject Point;

    public static readonly Dictionary<LobbyLocationType, LocationData> LocationDatas =
        new()
        {
            {
                LobbyLocationType.Tower1,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.Outside), OffsetX = 162.893f, OffsetY = 20f,
                    OffsetZ = -13.430f, RotationX = 0, RotationY = 270, RotationZ = 0
                }
            },
            {
                LobbyLocationType.Tower2,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.Outside), OffsetX = 108.03f, OffsetY = 15f, OffsetZ = -13.71f,
                    RotationX = 0, RotationY = 90, RotationZ = 0
                }
            },
            {
                LobbyLocationType.Tower3,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.Outside), OffsetX = 39.12f, OffsetY = 15f, OffsetZ = -32f,
                    RotationX = 0, RotationY = 270, RotationZ = 0
                }
            },
            {
                LobbyLocationType.Tower4,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.Outside), OffsetX = -15.854f, OffsetY = 15f,
                    OffsetZ = -31.543f, RotationX = 0, RotationY = 90, RotationZ = 0
                }
            },
            {
                LobbyLocationType.Tower5,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.Outside), OffsetX = 130.43f, OffsetY = -5.6f, OffsetZ = 21f,
                    RotationX = 0, RotationY = 180, RotationZ = 0
                }
            },
            {
                LobbyLocationType.Intercom,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.EzIntercom), OffsetX = -4.16f, OffsetY = -3.860f,
                    OffsetZ = -2.113f, RotationX = 0, RotationY = 180, RotationZ = 0
                }
            },
            {
                LobbyLocationType.Gr18,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.LczGlassroom), OffsetX = 4.8f, OffsetY = 1f, OffsetZ = 2.3f,
                    RotationX = 0, RotationY = 180, RotationZ = 0
                }
            },
            {
                LobbyLocationType.Scp173,
                new CustomRoomLocationData
                {
                    RoomNameType = nameof(RoomName.Lcz173), OffsetX = 17f, OffsetY = 13f, OffsetZ = 8f,
                    RotationX = 0, RotationY = -90, RotationZ = 0
                }
            }
        };

    public static void SetLocation(LocationData locationData)
    {
        if (locationData is CustomRoomLocationData customRoomLocation)
        {
            RoomIdentifier room;

            if (Enum.TryParse(customRoomLocation.RoomNameType, out RoomName roomName))
            {
                room = RoomIdentifier.AllRoomIdentifiers.First(x => x.Name == roomName);

                if (customRoomLocation.RoomNameType == nameof(RoomName.EzIntercom))
                    EventsHandler.IsIntercom = true;
            }
            else if (RoomIdentifier.AllRoomIdentifiers.Count(x =>
                         x.name.Contains(customRoomLocation.RoomNameType)) > 0)
            {
                room =
                    RoomIdentifier.AllRoomIdentifiers.First(x => x.name.Contains(customRoomLocation.RoomNameType));
            }
            else
            {
                customRoomLocation = (CustomRoomLocationData)LocationDatas[LobbyLocationType.Gr18];
                room = RoomIdentifier.AllRoomIdentifiers.First(x =>
                    x.Name == ParseEnum<RoomName>(customRoomLocation.RoomNameType));
            }

            Point.transform.SetParent(room.transform);
            Point.transform.localPosition = new Vector3(customRoomLocation.OffsetX, customRoomLocation.OffsetY,
                customRoomLocation.OffsetZ);
            Point.transform.localRotation = Quaternion.Euler(customRoomLocation.RotationX,
                customRoomLocation.RotationY, customRoomLocation.RotationZ);
        }
        else if (locationData is CustomLocationData customLocation)
        {
            Point.transform.localPosition = new Vector3(customLocation.PositionX, customLocation.PositionY,
                customLocation.PositionZ);
            Point.transform.localRotation = Quaternion.Euler(customLocation.RotationX, customLocation.RotationY,
                customLocation.RotationZ);
        }
    }

    private static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
}