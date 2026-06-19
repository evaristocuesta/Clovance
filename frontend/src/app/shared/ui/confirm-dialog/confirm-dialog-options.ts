import { IconName } from "../icon/icon-name";

export interface ConfirmDialogOptions {
  title: string;
  message: string;
  confirmText?: string;
  confirmIcon?: IconName;
  cancelText?: string;
  danger?: boolean;
}