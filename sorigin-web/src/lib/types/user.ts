import type DiscordUser from './discordUser'
import type SteamUser from './steamUser'

export enum GamePlatform {
    None,
    Steam
}

export default interface User {
    id: string
    username: string
    bio?: string
    gamePlatform: GamePlatform
    discord?: DiscordUser
    steam?: SteamUser
}