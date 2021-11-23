export default interface User {
	id: number;
	username: string;
	country: string | null;
	profilePicture: string;

	registration: Date;
	lastLogin: Date;
}
