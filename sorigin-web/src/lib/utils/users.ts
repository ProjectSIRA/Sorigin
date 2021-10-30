import type User from '$lib/types/user';
import { SORIGIN_URL } from './env';

export enum Size {
    Small,
    Medium,
    Large
}

export function getPFP(user: User, size: Size) {
    if (user.discord) {
        if (size === Size.Small) {
            return formatDiscordPFP(user.discord.avatarURL + '?size=128');
        }
        if (size === Size.Medium) {
            return formatDiscordPFP(user.discord.avatarURL + '?size=256');
        }
        if (size === Size.Large) {
            return formatDiscordPFP(user.discord.avatarURL + '?size=1024');
        }
    } else if (user.steam) {
        if (size === Size.Small) {
            return formatSteamPFP(user.steam.avatar) + '.jpg';
        }
        if (size === Size.Medium) {
            return formatSteamPFP(user.steam.avatar) + '_medium.jpg';
        }
        if (size === Size.Large) {
            return formatSteamPFP(user.steam.avatar) + '_full.jpg';
        }
    }
    return 'unknown';
}

function formatDiscordPFP(url: string) {
    return SORIGIN_URL + url;
}

function formatSteamPFP(hash: string) {
    return (
        'https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/' +
        `${hash.substring(0, 2)}/${hash}`
    );
}
