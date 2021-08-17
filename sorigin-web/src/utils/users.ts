import type User from 'src/types/user'

export enum Size {
    Small,
    Medium,
    Large
}

export function getPFP(user: User, size: Size) {
    if (user.discord !== null) {
        if (size === Size.Small) {
            return user.discord.avatarURL + "?size=128"
        }
        if (size === Size.Medium) {
            return user.discord.avatarURL + "?size=256"
        }
        if (size === Size.Large) {
            return user.discord.avatarURL + "?size=1024"
        }
    }
    else if (user.steam !== null) {
        if (size === Size.Small) {
            return formatSteamPFP(user.steam.avatar) + '.jpg'
        }
        if (size === Size.Medium) {
            return formatSteamPFP(user.steam.avatar) + '_medium.jpg'
        }
        if (size === Size.Large) {
            return formatSteamPFP(user.steam.avatar) + '_full.jpg'
        }
    }
    return 'unknown'
}

function formatSteamPFP(hash: string) {
    return 'https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/' + `${hash.substring(0, 2)}/${hash}`
}