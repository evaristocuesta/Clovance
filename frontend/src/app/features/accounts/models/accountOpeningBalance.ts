import { Account } from "./account.model";

export interface AccountOpeningBalance extends Account{
  openingBalance: number;
  openingDate: string;
  openingDescription: string;
}