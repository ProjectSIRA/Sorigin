import type DiscordUser from './discordUser'
import type SteamUser from './steamUser'

export enum GamePlatform {
    None,
    Steam
}

export enum Role {
    None = 0,
    Owner = 1 << 0,
    Admin = 1 << 1,
    Verified = 1 << 2
}

export default interface User {
    id: string
    role: Role
    username: string
    bio?: string
    gamePlatform: GamePlatform
    discord?: DiscordUser
    steam?: SteamUser
}